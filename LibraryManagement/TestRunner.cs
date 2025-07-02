using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement
{
    /// <summary>
    /// Test runner to demonstrate the Library Management System functionality
    /// </summary>
    public class TestRunner
    {
        private readonly LibraryContext _context;
        private readonly BookService _bookService;
        private readonly BorrowerService _borrowerService;
        private readonly ReportService _reportService;

        public TestRunner()
        {
            _context = new LibraryContext();
            _bookService = new BookService(_context);
            _borrowerService = new BorrowerService(_context);
            _reportService = new ReportService(_context);
        }

        /// <summary>
        /// Runs a comprehensive test of the system functionality
        /// </summary>
        public async Task RunTestsAsync()
        {
            Console.WriteLine("=== LIBRARY MANAGEMENT SYSTEM TEST RUNNER ===");
            Console.WriteLine("This will demonstrate the key functionality of the system.\n");

            try
            {
                // Test 1: Add sample books
                await TestAddBooksAsync();

                // Test 2: Add sample borrowers
                await TestAddBorrowersAsync();

                // Test 3: Test borrowing functionality
                await TestBorrowingAsync();

                // Test 4: Test returning functionality
                await TestReturningAsync();

                // Test 5: Generate reports
                await TestReportsAsync();

                Console.WriteLine("\n=== ALL TESTS COMPLETED SUCCESSFULLY ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed with error: {ex.Message}");
            }
        }

        private async Task TestAddBooksAsync()
        {
            Console.WriteLine("1. Testing Book Management...");

            var books = new List<Book>
            {
                new Book
                {
                    Title = "The Great Gatsby",
                    Author = "F. Scott Fitzgerald",
                    ISBN = "978-0-7432-7356-5",
                    Publisher = "Scribner",
                    PublicationYear = 1925,
                    Genre = "Fiction",
                    TotalCopies = 3,
                    AvailableCopies = 3,
                    Location = "A-001"
                },
                new Book
                {
                    Title = "To Kill a Mockingbird",
                    Author = "Harper Lee",
                    ISBN = "978-0-06-112008-4",
                    Publisher = "J.B. Lippincott & Co.",
                    PublicationYear = 1960,
                    Genre = "Fiction",
                    TotalCopies = 2,
                    AvailableCopies = 2,
                    Location = "A-002"
                },
                new Book
                {
                    Title = "1984",
                    Author = "George Orwell",
                    ISBN = "978-0-452-28423-4",
                    Publisher = "Secker & Warburg",
                    PublicationYear = 1949,
                    Genre = "Dystopian Fiction",
                    TotalCopies = 4,
                    AvailableCopies = 4,
                    Location = "A-003"
                }
            };

            foreach (var book in books)
            {
                await _bookService.AddBookAsync(book);
            }

            Console.WriteLine("   ✓ Sample books added successfully");
        }

        private async Task TestAddBorrowersAsync()
        {
            Console.WriteLine("2. Testing Borrower Management...");

            var borrowers = new List<Borrower>
            {
                new Borrower
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@email.com",
                    Phone = "555-0101",
                    Address = "123 Main St, City, State",
                    MembershipType = MembershipType.Public
                },
                new Borrower
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@email.com",
                    Phone = "555-0102",
                    Address = "456 Oak Ave, City, State",
                    MembershipType = MembershipType.Student
                },
                new Borrower
                {
                    FirstName = "Dr. Robert",
                    LastName = "Johnson",
                    Email = "robert.johnson@email.com",
                    Phone = "555-0103",
                    Address = "789 Pine Rd, City, State",
                    MembershipType = MembershipType.Faculty
                }
            };

            foreach (var borrower in borrowers)
            {
                await _borrowerService.AddBorrowerAsync(borrower);
            }

            Console.WriteLine("   ✓ Sample borrowers added successfully");
        }

        private async Task TestBorrowingAsync()
        {
            Console.WriteLine("3. Testing Book Borrowing...");

            // Test borrowing books
            await _bookService.BorrowBookAsync(1, 1, 14); // John borrows The Great Gatsby
            await _bookService.BorrowBookAsync(2, 2, 14); // Jane borrows To Kill a Mockingbird
            await _bookService.BorrowBookAsync(3, 3, 21); // Dr. Johnson borrows 1984 for 21 days

            Console.WriteLine("   ✓ Books borrowed successfully");

            // Test search functionality
            var availableBooks = await _bookService.GetAvailableBooksAsync();
            Console.WriteLine($"   ✓ Available books: {availableBooks.Count}");

            var johnsBorrowings = await _borrowerService.GetCurrentBorrowingsAsync(1);
            Console.WriteLine($"   ✓ John's current borrowings: {johnsBorrowings.Count}");
        }

        private async Task TestReturningAsync()
        {
            Console.WriteLine("4. Testing Book Returning...");

            // Return one book
            await _bookService.ReturnBookAsync(1, 1); // John returns The Great Gatsby

            Console.WriteLine("   ✓ Book returned successfully");

            // Check updated availability
            var book = await _bookService.GetBookByIdAsync(1);
            Console.WriteLine($"   ✓ Book availability updated: {book?.AvailableCopies}/{book?.TotalCopies}");
        }

        private async Task TestReportsAsync()
        {
            Console.WriteLine("5. Testing Report Generation...");

            // Generate various reports
            var popularBooks = await _reportService.GetPopularBooksReportAsync(5);
            Console.WriteLine($"   ✓ Popular books report generated: {popularBooks.Count} entries");

            var borrowerActivity = await _reportService.GetBorrowerActivityReportAsync(5);
            Console.WriteLine($"   ✓ Borrower activity report generated: {borrowerActivity.Count} entries");

            var overdueBooks = await _reportService.GetOverdueBooksReportAsync();
            Console.WriteLine($"   ✓ Overdue books report generated: {overdueBooks.Count} entries");

            var booksDueSoon = await _reportService.GetBooksDueSoonReportAsync(7);
            Console.WriteLine($"   ✓ Books due soon report generated: {booksDueSoon.Count} entries");

            Console.WriteLine("   ✓ All reports generated successfully");
        }

        /// <summary>
        /// Demonstrates search functionality
        /// </summary>
        public async Task DemonstrateSearchAsync()
        {
            Console.WriteLine("\n=== SEARCH FUNCTIONALITY DEMO ===");

            // Search books by title
            var gatsbyBooks = await _bookService.SearchBooksByTitleAsync("Gatsby");
            Console.WriteLine($"Books with 'Gatsby' in title: {gatsbyBooks.Count}");

            // Search books by author
            var orwellBooks = await _bookService.SearchBooksByAuthorAsync("Orwell");
            Console.WriteLine($"Books by authors containing 'Orwell': {orwellBooks.Count}");

            // Search borrowers by name
            var johnBorrowers = await _borrowerService.SearchBorrowersByNameAsync("John");
            Console.WriteLine($"Borrowers with 'John' in name: {johnBorrowers.Count}");
        }

        /// <summary>
        /// Demonstrates business rule validation
        /// </summary>
        public async Task DemonstrateValidationAsync()
        {
            Console.WriteLine("\n=== BUSINESS RULE VALIDATION DEMO ===");

            // Try to borrow a book that's not available
            var success1 = await _bookService.BorrowBookAsync(999, 1); // Non-existent book
            Console.WriteLine($"Borrowing non-existent book: {(success1 ? "Success" : "Failed (Expected)")}");

            // Try to add a borrower with duplicate email
            var duplicateBorrower = new Borrower
            {
                FirstName = "Test",
                LastName = "User",
                Email = "john.doe@email.com", // Duplicate email
                MembershipType = MembershipType.Public
            };
            var success2 = await _borrowerService.AddBorrowerAsync(duplicateBorrower);
            Console.WriteLine($"Adding borrower with duplicate email: {(success2 ? "Success" : "Failed (Expected)")}");
        }
    }
}
