using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Services
{
    /// <summary>
    /// Service class for managing borrower-related operations
    /// </summary>
    public class BorrowerService
    {
        private readonly LibraryContext _context;

        /// <summary>
        /// Constructor for BorrowerService
        /// </summary>
        /// <param name="context">Database context</param>
        public BorrowerService(LibraryContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new borrower to the system
        /// </summary>
        /// <param name="borrower">Borrower to add</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> AddBorrowerAsync(Borrower borrower)
        {
            try
            {
                // Check if borrower with same email already exists
                var existingBorrower = await _context.Borrowers.FirstOrDefaultAsync(b => b.Email == borrower.Email);
                if (existingBorrower != null)
                {
                    Console.WriteLine($"Borrower with email {borrower.Email} already exists.");
                    return false;
                }

                // Set default values based on membership type
                SetDefaultMaxBooks(borrower);

                _context.Borrowers.Add(borrower);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Borrower '{borrower.FullName}' added successfully with ID: {borrower.BorrowerId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding borrower: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates an existing borrower
        /// </summary>
        /// <param name="borrower">Borrower to update</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateBorrowerAsync(Borrower borrower)
        {
            try
            {
                _context.Borrowers.Update(borrower);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Borrower '{borrower.FullName}' updated successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating borrower: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets a borrower by ID
        /// </summary>
        /// <param name="borrowerId">Borrower ID</param>
        /// <returns>Borrower if found, null otherwise</returns>
        public async Task<Borrower?> GetBorrowerByIdAsync(int borrowerId)
        {
            try
            {
                return await _context.Borrowers
                    .Include(b => b.Transactions)
                    .ThenInclude(t => t.Book)
                    .FirstOrDefaultAsync(b => b.BorrowerId == borrowerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting borrower by ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Searches for borrowers by name
        /// </summary>
        /// <param name="name">Name to search for (first or last name)</param>
        /// <returns>List of matching borrowers</returns>
        public async Task<List<Borrower>> SearchBorrowersByNameAsync(string name)
        {
            try
            {
                return await _context.Borrowers
                    .Where(b => b.FirstName.Contains(name) || b.LastName.Contains(name))
                    .OrderBy(b => b.LastName)
                    .ThenBy(b => b.FirstName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching borrowers by name: {ex.Message}");
                return new List<Borrower>();
            }
        }

        /// <summary>
        /// Searches for a borrower by email
        /// </summary>
        /// <param name="email">Email to search for</param>
        /// <returns>Borrower if found, null otherwise</returns>
        public async Task<Borrower?> SearchBorrowerByEmailAsync(string email)
        {
            try
            {
                return await _context.Borrowers
                    .Include(b => b.Transactions)
                    .ThenInclude(t => t.Book)
                    .FirstOrDefaultAsync(b => b.Email == email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching borrower by email: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets all borrowers
        /// </summary>
        /// <returns>List of all borrowers</returns>
        public async Task<List<Borrower>> GetAllBorrowersAsync()
        {
            try
            {
                return await _context.Borrowers
                    .OrderBy(b => b.LastName)
                    .ThenBy(b => b.FirstName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all borrowers: {ex.Message}");
                return new List<Borrower>();
            }
        }

        /// <summary>
        /// Gets active borrowers only
        /// </summary>
        /// <returns>List of active borrowers</returns>
        public async Task<List<Borrower>> GetActiveBorrowersAsync()
        {
            try
            {
                return await _context.Borrowers
                    .Where(b => b.IsActive)
                    .OrderBy(b => b.LastName)
                    .ThenBy(b => b.FirstName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting active borrowers: {ex.Message}");
                return new List<Borrower>();
            }
        }

        /// <summary>
        /// Gets borrowers with overdue books
        /// </summary>
        /// <returns>List of borrowers with overdue books</returns>
        public async Task<List<Borrower>> GetBorrowersWithOverdueBooksAsync()
        {
            try
            {
                return await _context.Borrowers
                    .Include(b => b.Transactions)
                    .ThenInclude(t => t.Book)
                    .Where(b => b.Transactions.Any(t => t.Status == TransactionStatus.Overdue || 
                                                      (t.Status == TransactionStatus.Active && 
                                                       t.DueDate.HasValue && 
                                                       t.DueDate.Value < DateTime.Today)))
                    .OrderBy(b => b.LastName)
                    .ThenBy(b => b.FirstName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting borrowers with overdue books: {ex.Message}");
                return new List<Borrower>();
            }
        }

        /// <summary>
        /// Gets current borrowings for a borrower
        /// </summary>
        /// <param name="borrowerId">Borrower ID</param>
        /// <returns>List of current borrowings</returns>
        public async Task<List<Transaction>> GetCurrentBorrowingsAsync(int borrowerId)
        {
            try
            {
                return await _context.Transactions
                    .Include(t => t.Book)
                    .Where(t => t.BorrowerId == borrowerId && 
                               t.Status == TransactionStatus.Active && 
                               t.TransactionType == TransactionType.Borrow)
                    .OrderBy(t => t.DueDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current borrowings: {ex.Message}");
                return new List<Transaction>();
            }
        }

        /// <summary>
        /// Gets borrowing history for a borrower
        /// </summary>
        /// <param name="borrowerId">Borrower ID</param>
        /// <returns>List of all transactions for the borrower</returns>
        public async Task<List<Transaction>> GetBorrowingHistoryAsync(int borrowerId)
        {
            try
            {
                return await _context.Transactions
                    .Include(t => t.Book)
                    .Where(t => t.BorrowerId == borrowerId)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting borrowing history: {ex.Message}");
                return new List<Transaction>();
            }
        }

        /// <summary>
        /// Activates a borrower account
        /// </summary>
        /// <param name="borrowerId">Borrower ID</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ActivateBorrowerAsync(int borrowerId)
        {
            try
            {
                var borrower = await _context.Borrowers.FindAsync(borrowerId);
                if (borrower == null)
                {
                    Console.WriteLine("Borrower not found.");
                    return false;
                }

                borrower.IsActive = true;
                await _context.SaveChangesAsync();
                Console.WriteLine($"Borrower '{borrower.FullName}' activated successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error activating borrower: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deactivates a borrower account
        /// </summary>
        /// <param name="borrowerId">Borrower ID</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> DeactivateBorrowerAsync(int borrowerId)
        {
            try
            {
                var borrower = await _context.Borrowers
                    .Include(b => b.Transactions)
                    .FirstOrDefaultAsync(b => b.BorrowerId == borrowerId);

                if (borrower == null)
                {
                    Console.WriteLine("Borrower not found.");
                    return false;
                }

                // Check if borrower has active borrowings
                var activeBorrowings = borrower.Transactions
                    .Where(t => t.Status == TransactionStatus.Active && t.TransactionType == TransactionType.Borrow)
                    .Count();

                if (activeBorrowings > 0)
                {
                    Console.WriteLine($"Cannot deactivate borrower with {activeBorrowings} active borrowings.");
                    return false;
                }

                borrower.IsActive = false;
                await _context.SaveChangesAsync();
                Console.WriteLine($"Borrower '{borrower.FullName}' deactivated successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deactivating borrower: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deletes a borrower from the system
        /// </summary>
        /// <param name="borrowerId">Borrower ID</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> DeleteBorrowerAsync(int borrowerId)
        {
            try
            {
                var borrower = await _context.Borrowers
                    .Include(b => b.Transactions)
                    .FirstOrDefaultAsync(b => b.BorrowerId == borrowerId);

                if (borrower == null)
                {
                    Console.WriteLine("Borrower not found.");
                    return false;
                }

                // Check if borrower has any transactions
                if (borrower.Transactions.Any())
                {
                    Console.WriteLine("Cannot delete borrower with transaction history. Consider deactivating instead.");
                    return false;
                }

                _context.Borrowers.Remove(borrower);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Borrower '{borrower.FullName}' deleted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting borrower: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets default maximum books allowed based on membership type
        /// </summary>
        /// <param name="borrower">Borrower to set defaults for</param>
        private void SetDefaultMaxBooks(Borrower borrower)
        {
            switch (borrower.MembershipType)
            {
                case MembershipType.Student:
                    borrower.MaxBooksAllowed = 8;
                    break;
                case MembershipType.Faculty:
                    borrower.MaxBooksAllowed = 15;
                    break;
                case MembershipType.Staff:
                    borrower.MaxBooksAllowed = 10;
                    break;
                case MembershipType.Public:
                default:
                    borrower.MaxBooksAllowed = 5;
                    break;
            }
        }
    }
}
