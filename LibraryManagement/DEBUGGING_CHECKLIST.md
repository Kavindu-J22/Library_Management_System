# 🐛 Library Management System - Debugging Checklist

## Quick Reference Guide for Troubleshooting Common Issues

---

## 🔧 **Pre-Development Setup**

### Environment Verification
- [ ] **.NET 6+ SDK** installed and accessible via `dotnet --version`
- [ ] **MySQL Server** running (check with `mysql --version`)
- [ ] **Visual Studio 2022** or compatible IDE installed
- [ ] **MySQL Workbench** available for database management

### Project Setup
- [ ] **NuGet packages** restored (`dotnet restore`)
- [ ] **Project builds** successfully (`dotnet build`)
- [ ] **Connection string** configured in `LibraryContext.cs`
- [ ] **Database created** and schema applied

---

## 🗄️ **Database Connection Issues**

### Step 1: Check MySQL Service
```bash
# Windows
net start mysql80

# Check if MySQL is running
tasklist | findstr mysql
```

### Step 2: Test Connection String
```csharp
// Test this connection string format:
"Server=localhost;Database=LibraryManagement;Uid=root;Pwd=root;"
```

### Step 3: Common Connection Errors
- [ ] **Error 1045** (Access Denied)
  - ✅ Check username and password
  - ✅ Verify user has database permissions
  - ✅ Test login with MySQL Workbench

- [ ] **Error 1049** (Unknown Database)
  - ✅ Create database: `CREATE DATABASE LibraryManagement;`
  - ✅ Check database name spelling

- [ ] **Error 2003** (Can't Connect to Server)
  - ✅ Verify MySQL server is running
  - ✅ Check port number (default: 3306)
  - ✅ Check firewall settings

### Step 4: Database Schema Issues
- [ ] **Tables exist**: Run `SHOW TABLES;` in MySQL
- [ ] **Schema applied**: Execute `Database/library_schema.sql`
- [ ] **Foreign keys**: Check relationships are properly created
- [ ] **Sample data**: Verify test data is inserted

---

## 🔍 **Runtime Debugging Process**

### Step 1: Set Strategic Breakpoints
```csharp
public async Task<bool> AddBookAsync(Book book)
{
    try
    {
        // 🔴 Breakpoint 1: Check input parameters
        if (book == null) return false;
        
        // 🔴 Breakpoint 2: Before database operation
        _context.Books.Add(book);
        
        // 🔴 Breakpoint 3: After SaveChanges
        await _context.SaveChangesAsync();
        return true;
    }
    catch (Exception ex)
    {
        // 🔴 Breakpoint 4: Examine exception details
        Console.WriteLine($"Error: {ex.Message}");
        return false;
    }
}
```

### Step 2: Visual Studio Debugging Keys
- **F9**: Toggle breakpoint
- **F5**: Start debugging / Continue
- **F10**: Step over (execute current line)
- **F11**: Step into (enter method calls)
- **Shift+F11**: Step out (exit current method)
- **Ctrl+Shift+F10**: Run to cursor

### Step 3: Inspect Variables
- **Locals Window**: View all local variables
- **Watch Window**: Monitor specific expressions
- **Immediate Window**: Execute code during debugging
- **Call Stack**: View method call hierarchy

---

## ❌ **Common Error Patterns**

### 1. Null Reference Exceptions
```csharp
// ❌ Problem Code
var book = await _context.Books.FindAsync(bookId);
return book.Title; // NullReferenceException if book is null

// ✅ Solution
var book = await _context.Books.FindAsync(bookId);
if (book == null)
{
    Console.WriteLine($"Book {bookId} not found");
    return null;
}
return book.Title;
```

**Debugging Steps:**
- [ ] Set breakpoint before accessing object properties
- [ ] Check if object is null in Locals window
- [ ] Verify database query returns expected results
- [ ] Add null checks before property access

### 2. Entity Framework Exceptions
```csharp
// ❌ Problem: Constraint violations
try
{
    _context.Books.Add(book);
    await _context.SaveChangesAsync();
}
catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx)
{
    // ✅ Handle specific MySQL errors
    switch (mysqlEx.Number)
    {
        case 1062: // Duplicate entry
            Console.WriteLine("Book with this ISBN already exists");
            break;
        case 1452: // Foreign key constraint
            Console.WriteLine("Invalid foreign key reference");
            break;
    }
}
```

**Debugging Steps:**
- [ ] Check exception InnerException for MySQL details
- [ ] Verify entity validation attributes
- [ ] Check foreign key relationships
- [ ] Review database constraints

### 3. Business Logic Validation Errors
```csharp
// ✅ Comprehensive Validation with Debugging
public async Task<bool> BorrowBookAsync(int bookId, int borrowerId)
{
    // Step-by-step validation with logging
    Console.WriteLine($"Step 1: Validating book {bookId}");
    var book = await _context.Books.FindAsync(bookId);
    if (book == null)
    {
        Console.WriteLine("❌ Book not found");
        return false;
    }
    
    Console.WriteLine($"Step 2: Checking availability");
    if (!book.IsAvailable())
    {
        Console.WriteLine($"❌ Book not available. Copies: {book.AvailableCopies}");
        return false;
    }
    
    Console.WriteLine($"Step 3: Validating borrower {borrowerId}");
    // Continue with detailed validation...
}
```

**Debugging Steps:**
- [ ] Add console output for each validation step
- [ ] Set breakpoints at each business rule check
- [ ] Verify entity relationships are loaded (Include statements)
- [ ] Check calculated properties return expected values

---

## 🚀 **Performance Debugging**

### Identify Slow Queries
```csharp
var stopwatch = System.Diagnostics.Stopwatch.StartNew();

var books = await _context.Books
    .Include(b => b.Transactions)
    .ToListAsync();

stopwatch.Stop();
Console.WriteLine($"Query took {stopwatch.ElapsedMilliseconds}ms");

if (stopwatch.ElapsedMilliseconds > 1000)
{
    Console.WriteLine("⚠️ Performance issue detected");
}
```

**Performance Checklist:**
- [ ] **N+1 Query Problem**: Use `Include()` for related data
- [ ] **Large Datasets**: Implement pagination
- [ ] **Missing Indexes**: Add indexes for frequently queried columns
- [ ] **Unnecessary Data**: Only load required properties
- [ ] **Memory Leaks**: Dispose contexts properly

---

## 🖥️ **Console Application Debugging**

### Input Validation Issues
```csharp
// ✅ Robust Input Handling
Console.Write("Enter Book ID: ");
var input = Console.ReadLine();

if (!int.TryParse(input, out int bookId))
{
    Console.WriteLine("❌ Invalid input. Please enter a number.");
    return;
}

if (bookId <= 0)
{
    Console.WriteLine("❌ Book ID must be positive.");
    return;
}
```

**UI Debugging Steps:**
- [ ] Test all menu options
- [ ] Verify input validation handles edge cases
- [ ] Check error messages are user-friendly
- [ ] Test navigation between menus
- [ ] Verify data display formatting

---

## 📊 **Logging and Monitoring**

### Structured Logging Implementation
```csharp
public static class DebugLogger
{
    public static void LogInfo(string operation, string message)
    {
        Console.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} {operation}: {message}");
    }
    
    public static void LogError(string operation, Exception ex)
    {
        Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} {operation}: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    }
    
    public static void LogWarning(string operation, string message)
    {
        Console.WriteLine($"[WARN] {DateTime.Now:HH:mm:ss} {operation}: {message}");
    }
}

// Usage
DebugLogger.LogInfo("BookService.AddBook", $"Adding book: {book.Title}");
```

---

## 🔄 **Testing and Validation**

### Manual Testing Checklist
- [ ] **Add Book**: Test with valid and invalid data
- [ ] **Search Books**: Test various search criteria
- [ ] **Borrow Book**: Test availability and limits
- [ ] **Return Book**: Test overdue calculations
- [ ] **Generate Reports**: Verify data accuracy

### Automated Testing Approach
```csharp
// Example unit test structure
[Test]
public async Task AddBook_ValidBook_ReturnsTrue()
{
    // Arrange
    var book = new Book { Title = "Test", Author = "Author", ISBN = "123" };
    
    // Act
    var result = await _bookService.AddBookAsync(book);
    
    // Assert
    Assert.IsTrue(result);
}
```

---

## 🆘 **Emergency Debugging**

### When Everything Fails
1. **Check the Basics**
   - [ ] Is MySQL running?
   - [ ] Can you connect with MySQL Workbench?
   - [ ] Does the database exist?
   - [ ] Are tables created?

2. **Isolate the Problem**
   - [ ] Test database connection separately
   - [ ] Test individual methods in isolation
   - [ ] Use minimal test data
   - [ ] Check one feature at a time

3. **Get Help**
   - [ ] Check exception stack traces
   - [ ] Review MySQL error logs
   - [ ] Search for specific error codes
   - [ ] Use debugging examples in `Examples/DebuggingExamples.cs`

---

## 📝 **Quick Reference Commands**

### MySQL Commands
```sql
-- Check database exists
SHOW DATABASES;

-- Use database
USE LibraryManagement;

-- Check tables
SHOW TABLES;

-- Check table structure
DESCRIBE Books;

-- Check data
SELECT COUNT(*) FROM Books;
```

### .NET Commands
```bash
# Build project
dotnet build

# Run project
dotnet run

# Run with test data
dotnet run -- --test

# Clean and rebuild
dotnet clean && dotnet build
```

---

## ✅ **Success Indicators**

Your debugging is successful when:
- [ ] Application starts without errors
- [ ] Database connection is established
- [ ] All CRUD operations work correctly
- [ ] Business rules are enforced
- [ ] Error messages are clear and helpful
- [ ] Performance is acceptable
- [ ] User interface is responsive

Remember: **Good debugging is about systematic investigation, not random changes!**
