using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.UI
{
    /// <summary>
    /// Console-based user interface for the Library Management System
    /// </summary>
    public class ConsoleUI
    {
        private readonly BookService _bookService;
        private readonly BorrowerService _borrowerService;
        private readonly ReportService _reportService;
        private readonly LibraryContext _context;

        /// <summary>
        /// Constructor for ConsoleUI
        /// </summary>
        public ConsoleUI()
        {
            _context = new LibraryContext();
            _bookService = new BookService(_context);
            _borrowerService = new BorrowerService(_context);
            _reportService = new ReportService(_context);
        }

        /// <summary>
        /// Starts the main application loop
        /// </summary>
        public async Task RunAsync()
        {
            Console.WriteLine("=== LIBRARY MANAGEMENT SYSTEM ===");
            Console.WriteLine("Welcome to the Library Management System!");
            Console.WriteLine();

            bool exit = false;
            while (!exit)
            {
                try
                {
                    DisplayMainMenu();
                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            await BookManagementMenuAsync();
                            break;
                        case "2":
                            await BorrowerManagementMenuAsync();
                            break;
                        case "3":
                            await TransactionMenuAsync();
                            break;
                        case "4":
                            await ReportsMenuAsync();
                            break;
                        case "5":
                            exit = true;
                            Console.WriteLine("Thank you for using the Library Management System!");
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }

                    if (!exit)
                    {
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        /// <summary>
        /// Displays the main menu
        /// </summary>
        private void DisplayMainMenu()
        {
            Console.WriteLine("\n=== MAIN MENU ===");
            Console.WriteLine("1. Book Management");
            Console.WriteLine("2. Borrower Management");
            Console.WriteLine("3. Borrowing/Returning");
            Console.WriteLine("4. Reports");
            Console.WriteLine("5. Exit");
            Console.Write("Enter your choice (1-5): ");
        }

        /// <summary>
        /// Handles book management operations
        /// </summary>
        private async Task BookManagementMenuAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== BOOK MANAGEMENT ===");
            Console.WriteLine("1. Add New Book");
            Console.WriteLine("2. Search Books");
            Console.WriteLine("3. View All Books");
            Console.WriteLine("4. Update Book");
            Console.WriteLine("5. Delete Book");
            Console.WriteLine("6. Back to Main Menu");
            Console.Write("Enter your choice (1-6): ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await AddNewBookAsync();
                    break;
                case "2":
                    await SearchBooksAsync();
                    break;
                case "3":
                    await ViewAllBooksAsync();
                    break;
                case "4":
                    await UpdateBookAsync();
                    break;
                case "5":
                    await DeleteBookAsync();
                    break;
                case "6":
                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }

        /// <summary>
        /// Adds a new book to the library
        /// </summary>
        private async Task AddNewBookAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== ADD NEW BOOK ===");

            try
            {
                Console.Write("Enter book title: ");
                var title = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(title))
                {
                    Console.WriteLine("Title is required.");
                    return;
                }

                Console.Write("Enter author name: ");
                var author = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(author))
                {
                    Console.WriteLine("Author is required.");
                    return;
                }

                Console.Write("Enter ISBN: ");
                var isbn = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(isbn))
                {
                    Console.WriteLine("ISBN is required.");
                    return;
                }

                Console.Write("Enter publisher (optional): ");
                var publisher = Console.ReadLine();

                Console.Write("Enter publication year (optional): ");
                int? publicationYear = null;
                if (int.TryParse(Console.ReadLine(), out int year))
                {
                    publicationYear = year;
                }

                Console.Write("Enter genre (optional): ");
                var genre = Console.ReadLine();

                Console.Write("Enter total copies (default 1): ");
                int totalCopies = 1;
                if (int.TryParse(Console.ReadLine(), out int copies) && copies > 0)
                {
                    totalCopies = copies;
                }

                Console.Write("Enter shelf location (optional): ");
                var location = Console.ReadLine();

                var book = new Book
                {
                    Title = title,
                    Author = author,
                    ISBN = isbn,
                    Publisher = string.IsNullOrWhiteSpace(publisher) ? null : publisher,
                    PublicationYear = publicationYear,
                    Genre = string.IsNullOrWhiteSpace(genre) ? null : genre,
                    TotalCopies = totalCopies,
                    AvailableCopies = totalCopies,
                    Location = string.IsNullOrWhiteSpace(location) ? null : location
                };

                var success = await _bookService.AddBookAsync(book);
                if (success)
                {
                    Console.WriteLine("Book added successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding book: {ex.Message}");
            }
        }

        /// <summary>
        /// Searches for books
        /// </summary>
        private async Task SearchBooksAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== SEARCH BOOKS ===");
            Console.WriteLine("1. Search by Title");
            Console.WriteLine("2. Search by Author");
            Console.WriteLine("3. Search by ISBN");
            Console.Write("Enter your choice (1-3): ");

            var choice = Console.ReadLine();
            List<Book> books = new List<Book>();

            switch (choice)
            {
                case "1":
                    Console.Write("Enter title to search: ");
                    var title = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        books = await _bookService.SearchBooksByTitleAsync(title);
                    }
                    break;
                case "2":
                    Console.Write("Enter author to search: ");
                    var author = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(author))
                    {
                        books = await _bookService.SearchBooksByAuthorAsync(author);
                    }
                    break;
                case "3":
                    Console.Write("Enter ISBN to search: ");
                    var isbn = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(isbn))
                    {
                        var book = await _bookService.SearchBookByISBNAsync(isbn);
                        if (book != null)
                        {
                            books.Add(book);
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    return;
            }

            DisplayBooks(books);
        }

        /// <summary>
        /// Views all books in the library
        /// </summary>
        private async Task ViewAllBooksAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== ALL BOOKS ===");
            
            var books = await _bookService.GetAllBooksAsync();
            DisplayBooks(books);
        }

        /// <summary>
        /// Displays a list of books
        /// </summary>
        /// <param name="books">List of books to display</param>
        private void DisplayBooks(List<Book> books)
        {
            if (!books.Any())
            {
                Console.WriteLine("No books found.");
                return;
            }

            Console.WriteLine($"\nFound {books.Count} book(s):");
            Console.WriteLine(new string('=', 100));
            Console.WriteLine($"{"ID",-5} {"Title",-30} {"Author",-25} {"ISBN",-15} {"Available/Total",-15} {"Genre",-10}");
            Console.WriteLine(new string('-', 100));

            foreach (var book in books)
            {
                Console.WriteLine($"{book.BookId,-5} " +
                                $"{book.Title.Substring(0, Math.Min(29, book.Title.Length)),-30} " +
                                $"{book.Author.Substring(0, Math.Min(24, book.Author.Length)),-25} " +
                                $"{book.ISBN,-15} " +
                                $"{book.AvailableCopies}/{book.TotalCopies},-15 " +
                                $"{(book.Genre?.Substring(0, Math.Min(9, book.Genre?.Length ?? 0)) ?? "N/A"),-10}");
            }
        }

        /// <summary>
        /// Updates an existing book
        /// </summary>
        private async Task UpdateBookAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== UPDATE BOOK ===");
            
            Console.Write("Enter Book ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int bookId))
            {
                Console.WriteLine("Invalid Book ID.");
                return;
            }

            var book = await _bookService.GetBookByIdAsync(bookId);
            if (book == null)
            {
                Console.WriteLine("Book not found.");
                return;
            }

            Console.WriteLine($"Current book details: {book}");
            Console.WriteLine("Enter new values (press Enter to keep current value):");

            Console.Write($"Title ({book.Title}): ");
            var title = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(title))
                book.Title = title;

            Console.Write($"Author ({book.Author}): ");
            var author = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(author))
                book.Author = author;

            Console.Write($"Total Copies ({book.TotalCopies}): ");
            if (int.TryParse(Console.ReadLine(), out int totalCopies) && totalCopies > 0)
            {
                var difference = totalCopies - book.TotalCopies;
                book.TotalCopies = totalCopies;
                book.AvailableCopies = Math.Max(0, book.AvailableCopies + difference);
            }

            var success = await _bookService.UpdateBookAsync(book);
            if (success)
            {
                Console.WriteLine("Book updated successfully!");
            }
        }

        /// <summary>
        /// Deletes a book
        /// </summary>
        private async Task DeleteBookAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== DELETE BOOK ===");

            Console.Write("Enter Book ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int bookId))
            {
                Console.WriteLine("Invalid Book ID.");
                return;
            }

            var book = await _bookService.GetBookByIdAsync(bookId);
            if (book == null)
            {
                Console.WriteLine("Book not found.");
                return;
            }

            Console.WriteLine($"Are you sure you want to delete: {book}");
            Console.Write("Type 'YES' to confirm: ");
            var confirmation = Console.ReadLine();

            if (confirmation?.ToUpper() == "YES")
            {
                var success = await _bookService.DeleteBookAsync(bookId);
                if (success)
                {
                    Console.WriteLine("Book deleted successfully!");
                }
            }
            else
            {
                Console.WriteLine("Deletion cancelled.");
            }
        }

        /// <summary>
        /// Handles borrower management operations
        /// </summary>
        private async Task BorrowerManagementMenuAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== BORROWER MANAGEMENT ===");
            Console.WriteLine("1. Add New Borrower");
            Console.WriteLine("2. Search Borrowers");
            Console.WriteLine("3. View All Borrowers");
            Console.WriteLine("4. Update Borrower");
            Console.WriteLine("5. Activate/Deactivate Borrower");
            Console.WriteLine("6. View Borrower Details");
            Console.WriteLine("7. Back to Main Menu");
            Console.Write("Enter your choice (1-7): ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await AddNewBorrowerAsync();
                    break;
                case "2":
                    await SearchBorrowersAsync();
                    break;
                case "3":
                    await ViewAllBorrowersAsync();
                    break;
                case "4":
                    await UpdateBorrowerAsync();
                    break;
                case "5":
                    await ToggleBorrowerStatusAsync();
                    break;
                case "6":
                    await ViewBorrowerDetailsAsync();
                    break;
                case "7":
                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }

        /// <summary>
        /// Adds a new borrower
        /// </summary>
        private async Task AddNewBorrowerAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== ADD NEW BORROWER ===");

            try
            {
                Console.Write("Enter first name: ");
                var firstName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(firstName))
                {
                    Console.WriteLine("First name is required.");
                    return;
                }

                Console.Write("Enter last name: ");
                var lastName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(lastName))
                {
                    Console.WriteLine("Last name is required.");
                    return;
                }

                Console.Write("Enter email: ");
                var email = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine("Email is required.");
                    return;
                }

                Console.Write("Enter phone (optional): ");
                var phone = Console.ReadLine();

                Console.Write("Enter address (optional): ");
                var address = Console.ReadLine();

                Console.WriteLine("Select membership type:");
                Console.WriteLine("1. Public (5 books max)");
                Console.WriteLine("2. Student (8 books max)");
                Console.WriteLine("3. Faculty (15 books max)");
                Console.WriteLine("4. Staff (10 books max)");
                Console.Write("Enter choice (1-4): ");

                MembershipType membershipType = MembershipType.Public;
                var typeChoice = Console.ReadLine();
                switch (typeChoice)
                {
                    case "1":
                        membershipType = MembershipType.Public;
                        break;
                    case "2":
                        membershipType = MembershipType.Student;
                        break;
                    case "3":
                        membershipType = MembershipType.Faculty;
                        break;
                    case "4":
                        membershipType = MembershipType.Staff;
                        break;
                    default:
                        Console.WriteLine("Invalid choice, defaulting to Public.");
                        break;
                }

                var borrower = new Borrower
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                    Address = string.IsNullOrWhiteSpace(address) ? null : address,
                    MembershipType = membershipType,
                    MembershipDate = DateTime.Today,
                    IsActive = true
                };

                var success = await _borrowerService.AddBorrowerAsync(borrower);
                if (success)
                {
                    Console.WriteLine("Borrower added successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding borrower: {ex.Message}");
            }
        }

        /// <summary>
        /// Searches for borrowers
        /// </summary>
        private async Task SearchBorrowersAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== SEARCH BORROWERS ===");
            Console.WriteLine("1. Search by Name");
            Console.WriteLine("2. Search by Email");
            Console.Write("Enter your choice (1-2): ");

            var choice = Console.ReadLine();
            List<Borrower> borrowers = new List<Borrower>();

            switch (choice)
            {
                case "1":
                    Console.Write("Enter name to search: ");
                    var name = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        borrowers = await _borrowerService.SearchBorrowersByNameAsync(name);
                    }
                    break;
                case "2":
                    Console.Write("Enter email to search: ");
                    var email = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        var borrower = await _borrowerService.SearchBorrowerByEmailAsync(email);
                        if (borrower != null)
                        {
                            borrowers.Add(borrower);
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    return;
            }

            DisplayBorrowers(borrowers);
        }

        /// <summary>
        /// Views all borrowers
        /// </summary>
        private async Task ViewAllBorrowersAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== ALL BORROWERS ===");

            var borrowers = await _borrowerService.GetAllBorrowersAsync();
            DisplayBorrowers(borrowers);
        }

        /// <summary>
        /// Displays a list of borrowers
        /// </summary>
        /// <param name="borrowers">List of borrowers to display</param>
        private void DisplayBorrowers(List<Borrower> borrowers)
        {
            if (!borrowers.Any())
            {
                Console.WriteLine("No borrowers found.");
                return;
            }

            Console.WriteLine($"\nFound {borrowers.Count} borrower(s):");
            Console.WriteLine(new string('=', 100));
            Console.WriteLine($"{"ID",-5} {"Name",-25} {"Email",-30} {"Type",-10} {"Status",-10} {"Books",-10} {"Member Since",-12}");
            Console.WriteLine(new string('-', 100));

            foreach (var borrower in borrowers)
            {
                var status = borrower.IsActive ? "Active" : "Inactive";
                var booksInfo = $"{borrower.CurrentBorrowedBooks}/{borrower.MaxBooksAllowed}";
                Console.WriteLine($"{borrower.BorrowerId,-5} " +
                                $"{borrower.FullName.Substring(0, Math.Min(24, borrower.FullName.Length)),-25} " +
                                $"{borrower.Email.Substring(0, Math.Min(29, borrower.Email.Length)),-30} " +
                                $"{borrower.MembershipType,-10} " +
                                $"{status,-10} " +
                                $"{booksInfo,-10} " +
                                $"{borrower.MembershipDate:yyyy-MM-dd},-12");
            }
        }

        /// <summary>
        /// Updates an existing borrower
        /// </summary>
        private async Task UpdateBorrowerAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== UPDATE BORROWER ===");

            Console.Write("Enter Borrower ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int borrowerId))
            {
                Console.WriteLine("Invalid Borrower ID.");
                return;
            }

            var borrower = await _borrowerService.GetBorrowerByIdAsync(borrowerId);
            if (borrower == null)
            {
                Console.WriteLine("Borrower not found.");
                return;
            }

            Console.WriteLine($"Current borrower details: {borrower}");
            Console.WriteLine("Enter new values (press Enter to keep current value):");

            Console.Write($"First Name ({borrower.FirstName}): ");
            var firstName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(firstName))
                borrower.FirstName = firstName;

            Console.Write($"Last Name ({borrower.LastName}): ");
            var lastName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(lastName))
                borrower.LastName = lastName;

            Console.Write($"Phone ({borrower.Phone}): ");
            var phone = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(phone))
                borrower.Phone = phone;

            Console.Write($"Address ({borrower.Address}): ");
            var address = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(address))
                borrower.Address = address;

            var success = await _borrowerService.UpdateBorrowerAsync(borrower);
            if (success)
            {
                Console.WriteLine("Borrower updated successfully!");
            }
        }

        /// <summary>
        /// Toggles borrower active status
        /// </summary>
        private async Task ToggleBorrowerStatusAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== ACTIVATE/DEACTIVATE BORROWER ===");

            Console.Write("Enter Borrower ID: ");
            if (!int.TryParse(Console.ReadLine(), out int borrowerId))
            {
                Console.WriteLine("Invalid Borrower ID.");
                return;
            }

            var borrower = await _borrowerService.GetBorrowerByIdAsync(borrowerId);
            if (borrower == null)
            {
                Console.WriteLine("Borrower not found.");
                return;
            }

            Console.WriteLine($"Current status: {(borrower.IsActive ? "Active" : "Inactive")}");
            Console.WriteLine($"Borrower: {borrower.FullName}");

            if (borrower.IsActive)
            {
                Console.Write("Are you sure you want to deactivate this borrower? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    var success = await _borrowerService.DeactivateBorrowerAsync(borrowerId);
                    if (success)
                    {
                        Console.WriteLine("Borrower deactivated successfully!");
                    }
                }
            }
            else
            {
                Console.Write("Are you sure you want to activate this borrower? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    var success = await _borrowerService.ActivateBorrowerAsync(borrowerId);
                    if (success)
                    {
                        Console.WriteLine("Borrower activated successfully!");
                    }
                }
            }
        }

        /// <summary>
        /// Views detailed borrower information
        /// </summary>
        private async Task ViewBorrowerDetailsAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== BORROWER DETAILS ===");

            Console.Write("Enter Borrower ID: ");
            if (!int.TryParse(Console.ReadLine(), out int borrowerId))
            {
                Console.WriteLine("Invalid Borrower ID.");
                return;
            }

            var borrower = await _borrowerService.GetBorrowerByIdAsync(borrowerId);
            if (borrower == null)
            {
                Console.WriteLine("Borrower not found.");
                return;
            }

            Console.WriteLine($"\nBorrower Information:");
            Console.WriteLine($"ID: {borrower.BorrowerId}");
            Console.WriteLine($"Name: {borrower.FullName}");
            Console.WriteLine($"Email: {borrower.Email}");
            Console.WriteLine($"Phone: {borrower.Phone ?? "N/A"}");
            Console.WriteLine($"Address: {borrower.Address ?? "N/A"}");
            Console.WriteLine($"Membership Type: {borrower.MembershipType}");
            Console.WriteLine($"Member Since: {borrower.MembershipDate:yyyy-MM-dd}");
            Console.WriteLine($"Status: {(borrower.IsActive ? "Active" : "Inactive")}");
            Console.WriteLine($"Books Limit: {borrower.MaxBooksAllowed}");
            Console.WriteLine($"Current Books: {borrower.CurrentBorrowedBooks}");
            Console.WriteLine($"Total Fines: ${borrower.TotalFineAmount:F2}");

            // Show current borrowings
            var currentBorrowings = await _borrowerService.GetCurrentBorrowingsAsync(borrowerId);
            if (currentBorrowings.Any())
            {
                Console.WriteLine($"\nCurrent Borrowings ({currentBorrowings.Count}):");
                Console.WriteLine(new string('-', 80));
                Console.WriteLine($"{"Book Title",-35} {"Due Date",-12} {"Days Left",-10} {"Status",-10}");
                Console.WriteLine(new string('-', 80));

                foreach (var transaction in currentBorrowings)
                {
                    var daysLeft = transaction.DueDate.HasValue ?
                        (transaction.DueDate.Value - DateTime.Today).Days : 0;
                    var status = transaction.IsOverdue() ? "OVERDUE" : "Active";

                    Console.WriteLine($"{transaction.Book.Title.Substring(0, Math.Min(34, transaction.Book.Title.Length)),-35} " +
                                    $"{transaction.DueDate:yyyy-MM-dd},-12 " +
                                    $"{daysLeft,-10} " +
                                    $"{status,-10}");
                }
            }
            else
            {
                Console.WriteLine("\nNo current borrowings.");
            }
        }

        /// <summary>
        /// Handles borrowing and returning transactions
        /// </summary>
        private async Task TransactionMenuAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== BORROWING/RETURNING ===");
            Console.WriteLine("1. Borrow Book");
            Console.WriteLine("2. Return Book");
            Console.WriteLine("3. View Current Borrowings");
            Console.WriteLine("4. Back to Main Menu");
            Console.Write("Enter your choice (1-4): ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await BorrowBookAsync();
                    break;
                case "2":
                    await ReturnBookAsync();
                    break;
                case "3":
                    await ViewCurrentBorrowingsAsync();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }

        /// <summary>
        /// Handles book borrowing
        /// </summary>
        private async Task BorrowBookAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== BORROW BOOK ===");

            Console.Write("Enter Book ID: ");
            if (!int.TryParse(Console.ReadLine(), out int bookId))
            {
                Console.WriteLine("Invalid Book ID.");
                return;
            }

            Console.Write("Enter Borrower ID: ");
            if (!int.TryParse(Console.ReadLine(), out int borrowerId))
            {
                Console.WriteLine("Invalid Borrower ID.");
                return;
            }

            Console.Write("Enter number of days to borrow (default 14): ");
            int days = 14;
            if (int.TryParse(Console.ReadLine(), out int inputDays) && inputDays > 0)
            {
                days = inputDays;
            }

            var success = await _bookService.BorrowBookAsync(bookId, borrowerId, days);
            if (success)
            {
                Console.WriteLine("Book borrowed successfully!");
            }
        }

        /// <summary>
        /// Handles book returning
        /// </summary>
        private async Task ReturnBookAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== RETURN BOOK ===");

            Console.Write("Enter Book ID: ");
            if (!int.TryParse(Console.ReadLine(), out int bookId))
            {
                Console.WriteLine("Invalid Book ID.");
                return;
            }

            Console.Write("Enter Borrower ID: ");
            if (!int.TryParse(Console.ReadLine(), out int borrowerId))
            {
                Console.WriteLine("Invalid Borrower ID.");
                return;
            }

            var success = await _bookService.ReturnBookAsync(bookId, borrowerId);
            if (success)
            {
                Console.WriteLine("Book returned successfully!");
            }
        }

        /// <summary>
        /// Views current borrowings for a borrower
        /// </summary>
        private async Task ViewCurrentBorrowingsAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== CURRENT BORROWINGS ===");

            Console.Write("Enter Borrower ID: ");
            if (!int.TryParse(Console.ReadLine(), out int borrowerId))
            {
                Console.WriteLine("Invalid Borrower ID.");
                return;
            }

            var borrowings = await _borrowerService.GetCurrentBorrowingsAsync(borrowerId);

            if (!borrowings.Any())
            {
                Console.WriteLine("No current borrowings found.");
                return;
            }

            Console.WriteLine($"\nCurrent Borrowings ({borrowings.Count}):");
            Console.WriteLine(new string('=', 90));
            Console.WriteLine($"{"Book ID",-8} {"Title",-35} {"Due Date",-12} {"Days Left",-10} {"Status",-10} {"Fine",-8}");
            Console.WriteLine(new string('-', 90));

            foreach (var transaction in borrowings)
            {
                var daysLeft = transaction.DueDate.HasValue ?
                    (transaction.DueDate.Value - DateTime.Today).Days : 0;
                var status = transaction.IsOverdue() ? "OVERDUE" : "Active";

                Console.WriteLine($"{transaction.BookId,-8} " +
                                $"{transaction.Book.Title.Substring(0, Math.Min(34, transaction.Book.Title.Length)),-35} " +
                                $"{transaction.DueDate:yyyy-MM-dd},-12 " +
                                $"{daysLeft,-10} " +
                                $"{status,-10} " +
                                $"${transaction.FineAmount:F2},-8");
            }
        }

        /// <summary>
        /// Handles reports menu
        /// </summary>
        private async Task ReportsMenuAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== REPORTS ===");
            Console.WriteLine("1. Overdue Books Report");
            Console.WriteLine("2. Book Availability Report");
            Console.WriteLine("3. Popular Books Report");
            Console.WriteLine("4. Borrower Activity Report");
            Console.WriteLine("5. Fine Collection Report");
            Console.WriteLine("6. Books Due Soon");
            Console.WriteLine("7. Back to Main Menu");
            Console.Write("Enter your choice (1-7): ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await _reportService.PrintOverdueBooksReportAsync();
                    break;
                case "2":
                    await _reportService.PrintBookAvailabilityReportAsync();
                    break;
                case "3":
                    await ShowPopularBooksReportAsync();
                    break;
                case "4":
                    await ShowBorrowerActivityReportAsync();
                    break;
                case "5":
                    await ShowFineCollectionReportAsync();
                    break;
                case "6":
                    await ShowBooksDueSoonReportAsync();
                    break;
                case "7":
                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }

        /// <summary>
        /// Shows popular books report
        /// </summary>
        private async Task ShowPopularBooksReportAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== POPULAR BOOKS REPORT ===");

            var popularBooks = await _reportService.GetPopularBooksReportAsync();

            if (!popularBooks.Any())
            {
                Console.WriteLine("No borrowing data found.");
                return;
            }

            Console.WriteLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"{"Rank",-6} {"Title",-40} {"Author",-25} {"Borrows",-8}");
            Console.WriteLine(new string('-', 80));

            int rank = 1;
            foreach (dynamic book in popularBooks)
            {
                Console.WriteLine($"{rank,-6} " +
                                $"{book.Title.ToString().Substring(0, Math.Min(39, book.Title.ToString().Length)),-40} " +
                                $"{book.Author.ToString().Substring(0, Math.Min(24, book.Author.ToString().Length)),-25} " +
                                $"{book.BorrowCount,-8}");
                rank++;
            }
        }

        /// <summary>
        /// Shows borrower activity report
        /// </summary>
        private async Task ShowBorrowerActivityReportAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== BORROWER ACTIVITY REPORT ===");

            var activeBorrowers = await _reportService.GetBorrowerActivityReportAsync();

            if (!activeBorrowers.Any())
            {
                Console.WriteLine("No borrowing activity found.");
                return;
            }

            Console.WriteLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine(new string('=', 90));
            Console.WriteLine($"{"Rank",-6} {"Name",-30} {"Email",-30} {"Total",-8} {"Current",-8}");
            Console.WriteLine(new string('-', 90));

            int rank = 1;
            foreach (dynamic borrower in activeBorrowers)
            {
                Console.WriteLine($"{rank,-6} " +
                                $"{borrower.FullName.ToString().Substring(0, Math.Min(29, borrower.FullName.ToString().Length)),-30} " +
                                $"{borrower.Email.ToString().Substring(0, Math.Min(29, borrower.Email.ToString().Length)),-30} " +
                                $"{borrower.TotalBorrows,-8} " +
                                $"{borrower.CurrentBorrows,-8}");
                rank++;
            }
        }

        /// <summary>
        /// Shows fine collection report
        /// </summary>
        private async Task ShowFineCollectionReportAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== FINE COLLECTION REPORT ===");

            var fines = await _reportService.GetFineCollectionReportAsync();

            if (!fines.Any())
            {
                Console.WriteLine("No fines found.");
                return;
            }

            Console.WriteLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Total transactions with fines: {fines.Count}");
            Console.WriteLine(new string('=', 100));
            Console.WriteLine($"{"Borrower",-25} {"Book Title",-35} {"Due Date",-12} {"Fine Amount",-12} {"Status",-10}");
            Console.WriteLine(new string('-', 100));

            foreach (var transaction in fines)
            {
                Console.WriteLine($"{transaction.Borrower.FullName.Substring(0, Math.Min(24, transaction.Borrower.FullName.Length)),-25} " +
                                $"{transaction.Book.Title.Substring(0, Math.Min(34, transaction.Book.Title.Length)),-35} " +
                                $"{transaction.DueDate:yyyy-MM-dd},-12 " +
                                $"${transaction.FineAmount:F2},-12 " +
                                $"{transaction.Status,-10}");
            }

            var totalFines = fines.Sum(f => f.FineAmount);
            Console.WriteLine(new string('-', 100));
            Console.WriteLine($"Total fines: ${totalFines:F2}");
        }

        /// <summary>
        /// Shows books due soon report
        /// </summary>
        private async Task ShowBooksDueSoonReportAsync()
        {
            Console.Clear();
            Console.WriteLine("\n=== BOOKS DUE SOON REPORT ===");

            Console.Write("Enter number of days to look ahead (default 3): ");
            int days = 3;
            if (int.TryParse(Console.ReadLine(), out int inputDays) && inputDays > 0)
            {
                days = inputDays;
            }

            var dueSoon = await _reportService.GetBooksDueSoonReportAsync(days);

            if (!dueSoon.Any())
            {
                Console.WriteLine($"No books due within {days} days.");
                return;
            }

            Console.WriteLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Books due within {days} days: {dueSoon.Count}");
            Console.WriteLine(new string('=', 90));
            Console.WriteLine($"{"Borrower",-25} {"Book Title",-35} {"Due Date",-12} {"Days Left",-10}");
            Console.WriteLine(new string('-', 90));

            foreach (var transaction in dueSoon)
            {
                var daysLeft = transaction.DueDate.HasValue ?
                    (transaction.DueDate.Value - DateTime.Today).Days : 0;

                Console.WriteLine($"{transaction.Borrower.FullName.Substring(0, Math.Min(24, transaction.Borrower.FullName.Length)),-25} " +
                                $"{transaction.Book.Title.Substring(0, Math.Min(34, transaction.Book.Title.Length)),-35} " +
                                $"{transaction.DueDate:yyyy-MM-dd},-12 " +
                                $"{daysLeft,-10}");
            }
        }
    }
}
