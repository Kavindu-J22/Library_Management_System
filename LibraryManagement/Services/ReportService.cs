using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Services
{
    /// <summary>
    /// Service class for generating various reports
    /// </summary>
    public class ReportService
    {
        private readonly LibraryContext _context;

        /// <summary>
        /// Constructor for ReportService
        /// </summary>
        /// <param name="context">Database context</param>
        public ReportService(LibraryContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generates a report of overdue books
        /// </summary>
        /// <returns>List of overdue transactions</returns>
        public async Task<List<Transaction>> GetOverdueBooksReportAsync()
        {
            try
            {
                var overdueTransactions = await _context.Transactions
                    .Include(t => t.Book)
                    .Include(t => t.Borrower)
                    .Where(t => t.Status == TransactionStatus.Active &&
                               t.TransactionType == TransactionType.Borrow &&
                               t.DueDate.HasValue &&
                               t.DueDate.Value < DateTime.Today)
                    .OrderBy(t => t.DueDate)
                    .ToListAsync();

                // Update status to overdue and calculate fines
                foreach (var transaction in overdueTransactions)
                {
                    if (transaction.Status != TransactionStatus.Overdue)
                    {
                        transaction.MarkAsOverdue();
                    }
                }

                if (overdueTransactions.Any())
                {
                    await _context.SaveChangesAsync();
                }

                return overdueTransactions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating overdue books report: {ex.Message}");
                return new List<Transaction>();
            }
        }

        /// <summary>
        /// Generates a report of books due soon (within specified days)
        /// </summary>
        /// <param name="daysAhead">Number of days to look ahead (default: 3)</param>
        /// <returns>List of transactions with books due soon</returns>
        public async Task<List<Transaction>> GetBooksDueSoonReportAsync(int daysAhead = 3)
        {
            try
            {
                var dueSoonDate = DateTime.Today.AddDays(daysAhead);
                
                return await _context.Transactions
                    .Include(t => t.Book)
                    .Include(t => t.Borrower)
                    .Where(t => t.Status == TransactionStatus.Active &&
                               t.TransactionType == TransactionType.Borrow &&
                               t.DueDate.HasValue &&
                               t.DueDate.Value >= DateTime.Today &&
                               t.DueDate.Value <= dueSoonDate)
                    .OrderBy(t => t.DueDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating books due soon report: {ex.Message}");
                return new List<Transaction>();
            }
        }

        /// <summary>
        /// Generates a book availability report
        /// </summary>
        /// <returns>List of books with availability information</returns>
        public async Task<List<Book>> GetBookAvailabilityReportAsync()
        {
            try
            {
                return await _context.Books
                    .OrderBy(b => b.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating book availability report: {ex.Message}");
                return new List<Book>();
            }
        }

        /// <summary>
        /// Generates a popular books report based on borrowing frequency
        /// </summary>
        /// <param name="topCount">Number of top books to return (default: 10)</param>
        /// <returns>List of popular books with borrow count</returns>
        public async Task<List<object>> GetPopularBooksReportAsync(int topCount = 10)
        {
            try
            {
                var popularBooks = await _context.Transactions
                    .Include(t => t.Book)
                    .Where(t => t.TransactionType == TransactionType.Borrow)
                    .GroupBy(t => new { t.BookId, t.Book.Title, t.Book.Author })
                    .Select(g => new
                    {
                        BookId = g.Key.BookId,
                        Title = g.Key.Title,
                        Author = g.Key.Author,
                        BorrowCount = g.Count()
                    })
                    .OrderByDescending(x => x.BorrowCount)
                    .Take(topCount)
                    .ToListAsync();

                return popularBooks.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating popular books report: {ex.Message}");
                return new List<object>();
            }
        }

        /// <summary>
        /// Generates a borrower activity report
        /// </summary>
        /// <param name="topCount">Number of top borrowers to return (default: 10)</param>
        /// <returns>List of active borrowers with borrow count</returns>
        public async Task<List<object>> GetBorrowerActivityReportAsync(int topCount = 10)
        {
            try
            {
                var activeBorrowers = await _context.Transactions
                    .Include(t => t.Borrower)
                    .Where(t => t.TransactionType == TransactionType.Borrow)
                    .GroupBy(t => new { t.BorrowerId, t.Borrower.FirstName, t.Borrower.LastName, t.Borrower.Email })
                    .Select(g => new
                    {
                        BorrowerId = g.Key.BorrowerId,
                        FullName = $"{g.Key.FirstName} {g.Key.LastName}",
                        Email = g.Key.Email,
                        TotalBorrows = g.Count(),
                        CurrentBorrows = g.Count(t => t.Status == TransactionStatus.Active)
                    })
                    .OrderByDescending(x => x.TotalBorrows)
                    .Take(topCount)
                    .ToListAsync();

                return activeBorrowers.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating borrower activity report: {ex.Message}");
                return new List<object>();
            }
        }

        /// <summary>
        /// Generates a fine collection report
        /// </summary>
        /// <returns>List of transactions with fines</returns>
        public async Task<List<Transaction>> GetFineCollectionReportAsync()
        {
            try
            {
                return await _context.Transactions
                    .Include(t => t.Book)
                    .Include(t => t.Borrower)
                    .Where(t => t.FineAmount > 0)
                    .OrderByDescending(t => t.FineAmount)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating fine collection report: {ex.Message}");
                return new List<Transaction>();
            }
        }

        /// <summary>
        /// Generates a monthly borrowing statistics report
        /// </summary>
        /// <param name="year">Year for the report</param>
        /// <param name="month">Month for the report (optional, if null returns yearly data)</param>
        /// <returns>Borrowing statistics</returns>
        public async Task<object> GetBorrowingStatisticsAsync(int year, int? month = null)
        {
            try
            {
                var query = _context.Transactions
                    .Where(t => t.TransactionDate.Year == year);

                if (month.HasValue)
                {
                    query = query.Where(t => t.TransactionDate.Month == month.Value);
                }

                var statistics = await query
                    .GroupBy(t => t.TransactionType)
                    .Select(g => new
                    {
                        TransactionType = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .ToListAsync();

                var totalFines = await query
                    .Where(t => t.FineAmount > 0)
                    .SumAsync(t => t.FineAmount);

                return new
                {
                    Year = year,
                    Month = month,
                    Statistics = statistics,
                    TotalFinesCollected = totalFines
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating borrowing statistics: {ex.Message}");
                return new { Error = ex.Message };
            }
        }

        /// <summary>
        /// Generates a genre popularity report
        /// </summary>
        /// <returns>List of genres with borrow count</returns>
        public async Task<List<object>> GetGenrePopularityReportAsync()
        {
            try
            {
                var genreStats = await _context.Transactions
                    .Include(t => t.Book)
                    .Where(t => t.TransactionType == TransactionType.Borrow && !string.IsNullOrEmpty(t.Book.Genre))
                    .GroupBy(t => t.Book.Genre)
                    .Select(g => new
                    {
                        Genre = g.Key,
                        BorrowCount = g.Count(),
                        UniqueBooks = g.Select(t => t.BookId).Distinct().Count()
                    })
                    .OrderByDescending(x => x.BorrowCount)
                    .ToListAsync();

                return genreStats.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating genre popularity report: {ex.Message}");
                return new List<object>();
            }
        }

        /// <summary>
        /// Prints overdue books report to console
        /// </summary>
        public async Task PrintOverdueBooksReportAsync()
        {
            var overdueBooks = await GetOverdueBooksReportAsync();
            
            Console.WriteLine("\n=== OVERDUE BOOKS REPORT ===");
            Console.WriteLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Total overdue books: {overdueBooks.Count}");
            Console.WriteLine(new string('=', 80));

            if (!overdueBooks.Any())
            {
                Console.WriteLine("No overdue books found.");
                return;
            }

            Console.WriteLine($"{"Title",-30} {"Borrower",-25} {"Due Date",-12} {"Days Overdue",-12} {"Fine",-10}");
            Console.WriteLine(new string('-', 80));

            foreach (var transaction in overdueBooks)
            {
                var daysOverdue = transaction.DaysOverdue();
                Console.WriteLine($"{transaction.Book.Title.Substring(0, Math.Min(29, transaction.Book.Title.Length)),-30} " +
                                $"{transaction.Borrower.FullName.Substring(0, Math.Min(24, transaction.Borrower.FullName.Length)),-25} " +
                                $"{transaction.DueDate:yyyy-MM-dd},-12 " +
                                $"{daysOverdue,-12} " +
                                $"${transaction.FineAmount:F2},-10");
            }

            var totalFines = overdueBooks.Sum(t => t.FineAmount);
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"Total fines: ${totalFines:F2}");
        }

        /// <summary>
        /// Prints book availability report to console
        /// </summary>
        public async Task PrintBookAvailabilityReportAsync()
        {
            var books = await GetBookAvailabilityReportAsync();
            
            Console.WriteLine("\n=== BOOK AVAILABILITY REPORT ===");
            Console.WriteLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Total books: {books.Count}");
            Console.WriteLine(new string('=', 90));

            Console.WriteLine($"{"Title",-35} {"Author",-25} {"Available",-10} {"Total",-8} {"Status",-10}");
            Console.WriteLine(new string('-', 90));

            foreach (var book in books)
            {
                var status = book.AvailableCopies > 0 ? "Available" : "Unavailable";
                Console.WriteLine($"{book.Title.Substring(0, Math.Min(34, book.Title.Length)),-35} " +
                                $"{book.Author.Substring(0, Math.Min(24, book.Author.Length)),-25} " +
                                $"{book.AvailableCopies,-10} " +
                                $"{book.TotalCopies,-8} " +
                                $"{status,-10}");
            }
        }
    }
}
