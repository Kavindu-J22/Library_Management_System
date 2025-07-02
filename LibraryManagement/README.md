# Library Management System

A comprehensive Library Management System built with C# .NET 6+, Entity Framework Core, and MySQL database. This system provides complete functionality for managing books, borrowers, and transactions in a library environment.

## 🚀 Features

### Book Management
- ✅ Add new books with detailed information (Title, Author, ISBN, Publisher, etc.)
- ✅ Search books by title, author, or ISBN
- ✅ Update book information and copy counts
- ✅ Track available and total copies
- ✅ Delete books (with validation for active transactions)

### Borrower Management
- ✅ Register new borrowers with different membership types
- ✅ Search borrowers by name or email
- ✅ Update borrower information
- ✅ Activate/deactivate borrower accounts
- ✅ View detailed borrower profiles with current borrowings

### Transaction Management
- ✅ Borrow books with automatic due date calculation
- ✅ Return books with overdue fine calculation
- ✅ Track borrowing history
- ✅ Automatic status updates (Active, Overdue, Completed)

### Reporting System
- ✅ Overdue books report with fine calculations
- ✅ Book availability report
- ✅ Popular books report (most borrowed)
- ✅ Borrower activity report
- ✅ Fine collection report
- ✅ Books due soon notifications

## 🏗️ Architecture

The system follows Object-Oriented Programming principles with a clean, layered architecture:

```
LibraryManagement/
├── Models/           # Entity classes (Book, Borrower, Transaction)
├── Data/            # Entity Framework DbContext
├── Services/        # Business logic layer
├── UI/              # Console user interface
└── Database/        # SQL schema files
```

### Key Components

- **Models**: Entity classes with data annotations and business logic methods
- **Data Layer**: Entity Framework Core context with MySQL provider
- **Service Layer**: Business logic for books, borrowers, and reports
- **UI Layer**: Console-based user interface with menu system

## 🛠️ Technology Stack

- **Framework**: .NET 6+
- **Database**: MySQL
- **ORM**: Entity Framework Core
- **MySQL Provider**: Pomelo.EntityFrameworkCore.MySql
- **IDE**: Visual Studio 2022 (recommended)

## 📋 Prerequisites

- .NET 6 SDK or later
- MySQL Server 8.0 or later
- Visual Studio 2022 or VS Code
- MySQL Workbench (optional, for database management)

## ⚙️ Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd LibraryManagement
```

### 2. Database Setup
1. Install MySQL Server and create a database:
```sql
CREATE DATABASE LibraryManagement;
```

2. Update the connection string in `Data/LibraryContext.cs`:
```csharp
string connectionString = "Server=localhost;Database=LibraryManagement;Uid=root;Pwd=root;";
```

3. Run the provided SQL schema:
```bash
mysql -u root -p LibraryManagement < Database/library_schema.sql
```

### 3. Install Dependencies
```bash
dotnet restore
```

### 4. Build the Project
```bash
dotnet build
```

### 5. Run the Application
```bash
dotnet run
```

## 📊 Database Schema

The system uses three main tables with proper relationships:

### Books Table
- `BookId` (Primary Key)
- `Title`, `Author`, `ISBN` (Required)
- `Publisher`, `PublicationYear`, `Genre`
- `TotalCopies`, `AvailableCopies`
- `Location`, `CreatedDate`, `UpdatedDate`

### Borrowers Table
- `BorrowerId` (Primary Key)
- `FirstName`, `LastName`, `Email` (Required)
- `Phone`, `Address`
- `MembershipType` (Student, Faculty, Public, Staff)
- `MembershipDate`, `IsActive`, `MaxBooksAllowed`

### Transactions Table
- `TransactionId` (Primary Key)
- `BookId`, `BorrowerId` (Foreign Keys)
- `TransactionType` (Borrow, Return, Renew)
- `TransactionDate`, `DueDate`, `ReturnDate`
- `FineAmount`, `Status`, `Notes`

## 🎯 Usage Examples

### Adding a New Book
```csharp
var book = new Book
{
    Title = "The Great Gatsby",
    Author = "F. Scott Fitzgerald",
    ISBN = "978-0-7432-7356-5",
    TotalCopies = 3,
    AvailableCopies = 3
};
await bookService.AddBookAsync(book);
```

### Borrowing a Book
```csharp
// Borrow book ID 1 for borrower ID 1, due in 14 days
await bookService.BorrowBookAsync(1, 1, 14);
```

### Generating Reports
```csharp
// Get overdue books
var overdueBooks = await reportService.GetOverdueBooksReportAsync();

// Print report to console
await reportService.PrintOverdueBooksReportAsync();
```

## 🔧 Configuration

### Membership Types & Limits
- **Public**: 5 books maximum
- **Student**: 8 books maximum
- **Faculty**: 15 books maximum
- **Staff**: 10 books maximum

### Fine Calculation
- Default: $0.50 per day overdue
- Automatically calculated when books are returned late
- Configurable in the `Transaction.CalculateFine()` method

## 🧪 Testing

The system includes comprehensive business logic with validation:

- Book availability checking
- Borrower limit enforcement
- Overdue detection and fine calculation
- Data integrity constraints

To test the system:
1. Run the application
2. Use the console menu to perform operations
3. Check the database for data consistency

## 📝 Code Examples

### Key Business Logic Methods

<augment_code_snippet path="LibraryManagement/Services/BookService.cs" mode="EXCERPT">
````csharp
public async Task<bool> BorrowBookAsync(int bookId, int borrowerId, int daysToReturn = 14)
{
    // Check availability, update DB, set due date
    var book = await _context.Books.FindAsync(bookId);
    var borrower = await _context.Borrowers.Include(b => b.Transactions)
        .FirstOrDefaultAsync(b => b.BorrowerId == borrowerId);
    
    if (!book.IsAvailable() || !borrower.CanBorrowMoreBooks())
        return false;
        
    // Create transaction and update book availability
    var transaction = new Transaction
    {
        BookId = bookId,
        BorrowerId = borrowerId,
        TransactionType = TransactionType.Borrow,
        DueDate = DateTime.Today.AddDays(daysToReturn)
    };
    
    book.BorrowCopy();
    _context.Transactions.Add(transaction);
    await _context.SaveChangesAsync();
    return true;
}
````
</augment_code_snippet>

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:
- Check the documentation
- Review the code comments
- Create an issue in the repository

## 🔮 Future Enhancements

- Web-based UI using ASP.NET Core
- REST API for mobile applications
- Advanced reporting with charts
- Email notifications for due dates
- Barcode scanning integration
- Multi-library support
