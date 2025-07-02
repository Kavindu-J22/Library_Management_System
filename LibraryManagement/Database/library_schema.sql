-- Library Management System Database Schema
-- MySQL Database Schema for Library Management System

-- Create database
CREATE DATABASE IF NOT EXISTS LibraryManagement;
USE LibraryManagement;

-- Table: Books
-- Stores information about books in the library
CREATE TABLE Books (
    BookId INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Author VARCHAR(255) NOT NULL,
    ISBN VARCHAR(20) UNIQUE NOT NULL,
    Publisher VARCHAR(255),
    PublicationYear INT,
    Genre VARCHAR(100),
    TotalCopies INT NOT NULL DEFAULT 1,
    AvailableCopies INT NOT NULL DEFAULT 1,
    Location VARCHAR(100), -- Shelf location in library
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Constraints
    CONSTRAINT chk_publication_year CHECK (PublicationYear > 1000 AND PublicationYear <= YEAR(CURDATE())),
    CONSTRAINT chk_total_copies CHECK (TotalCopies >= 0),
    CONSTRAINT chk_available_copies CHECK (AvailableCopies >= 0 AND AvailableCopies <= TotalCopies)
);

-- Table: Borrowers
-- Stores information about library members who can borrow books
CREATE TABLE Borrowers (
    BorrowerId INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    Phone VARCHAR(20),
    Address TEXT,
    MembershipDate DATE NOT NULL DEFAULT (CURDATE()),
    MembershipType ENUM('Student', 'Faculty', 'Public', 'Staff') NOT NULL DEFAULT 'Public',
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    MaxBooksAllowed INT NOT NULL DEFAULT 5,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Constraints
    CONSTRAINT chk_max_books CHECK (MaxBooksAllowed > 0 AND MaxBooksAllowed <= 20)
);

-- Table: Transactions
-- Stores borrowing and returning transactions
CREATE TABLE Transactions (
    TransactionId INT AUTO_INCREMENT PRIMARY KEY,
    BookId INT NOT NULL,
    BorrowerId INT NOT NULL,
    TransactionType ENUM('Borrow', 'Return', 'Renew') NOT NULL,
    TransactionDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DueDate DATE,
    ReturnDate DATE,
    FineAmount DECIMAL(10,2) DEFAULT 0.00,
    Status ENUM('Active', 'Completed', 'Overdue', 'Lost') NOT NULL DEFAULT 'Active',
    Notes TEXT,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Foreign Key Constraints
    CONSTRAINT fk_transaction_book FOREIGN KEY (BookId) REFERENCES Books(BookId) ON DELETE CASCADE,
    CONSTRAINT fk_transaction_borrower FOREIGN KEY (BorrowerId) REFERENCES Borrowers(BorrowerId) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT chk_fine_amount CHECK (FineAmount >= 0),
    CONSTRAINT chk_return_date CHECK (ReturnDate IS NULL OR ReturnDate >= DATE(TransactionDate))
);

-- Create indexes for better performance
CREATE INDEX idx_books_title ON Books(Title);
CREATE INDEX idx_books_author ON Books(Author);
CREATE INDEX idx_books_isbn ON Books(ISBN);
CREATE INDEX idx_books_genre ON Books(Genre);

CREATE INDEX idx_borrowers_email ON Borrowers(Email);
CREATE INDEX idx_borrowers_name ON Borrowers(LastName, FirstName);
CREATE INDEX idx_borrowers_membership ON Borrowers(MembershipType, IsActive);

CREATE INDEX idx_transactions_book ON Transactions(BookId);
CREATE INDEX idx_transactions_borrower ON Transactions(BorrowerId);
CREATE INDEX idx_transactions_status ON Transactions(Status);
CREATE INDEX idx_transactions_due_date ON Transactions(DueDate);
CREATE INDEX idx_transactions_type_date ON Transactions(TransactionType, TransactionDate);

-- Insert sample data for testing
INSERT INTO Books (Title, Author, ISBN, Publisher, PublicationYear, Genre, TotalCopies, AvailableCopies, Location) VALUES
('The Great Gatsby', 'F. Scott Fitzgerald', '978-0-7432-7356-5', 'Scribner', 1925, 'Fiction', 3, 3, 'A-001'),
('To Kill a Mockingbird', 'Harper Lee', '978-0-06-112008-4', 'J.B. Lippincott & Co.', 1960, 'Fiction', 2, 2, 'A-002'),
('1984', 'George Orwell', '978-0-452-28423-4', 'Secker & Warburg', 1949, 'Dystopian Fiction', 4, 4, 'A-003'),
('Pride and Prejudice', 'Jane Austen', '978-0-14-143951-8', 'T. Egerton', 1813, 'Romance', 2, 2, 'A-004'),
('The Catcher in the Rye', 'J.D. Salinger', '978-0-316-76948-0', 'Little, Brown and Company', 1951, 'Fiction', 3, 3, 'A-005');

INSERT INTO Borrowers (FirstName, LastName, Email, Phone, Address, MembershipType, MaxBooksAllowed) VALUES
('John', 'Doe', 'john.doe@email.com', '555-0101', '123 Main St, City, State', 'Public', 5),
('Jane', 'Smith', 'jane.smith@email.com', '555-0102', '456 Oak Ave, City, State', 'Student', 8),
('Robert', 'Johnson', 'robert.johnson@email.com', '555-0103', '789 Pine Rd, City, State', 'Faculty', 15),
('Emily', 'Davis', 'emily.davis@email.com', '555-0104', '321 Elm St, City, State', 'Staff', 10),
('Michael', 'Wilson', 'michael.wilson@email.com', '555-0105', '654 Maple Dr, City, State', 'Public', 5);

-- Create a view for overdue books
CREATE VIEW OverdueBooks AS
SELECT 
    t.TransactionId,
    b.Title,
    b.Author,
    b.ISBN,
    br.FirstName,
    br.LastName,
    br.Email,
    t.TransactionDate,
    t.DueDate,
    DATEDIFF(CURDATE(), t.DueDate) AS DaysOverdue,
    t.FineAmount
FROM Transactions t
JOIN Books b ON t.BookId = b.BookId
JOIN Borrowers br ON t.BorrowerId = br.BorrowerId
WHERE t.Status = 'Active' 
  AND t.DueDate < CURDATE()
  AND t.TransactionType = 'Borrow';

-- Create a view for current borrowings
CREATE VIEW CurrentBorrowings AS
SELECT 
    t.TransactionId,
    b.Title,
    b.Author,
    br.FirstName,
    br.LastName,
    br.Email,
    t.TransactionDate,
    t.DueDate,
    t.Status
FROM Transactions t
JOIN Books b ON t.BookId = b.BookId
JOIN Borrowers br ON t.BorrowerId = br.BorrowerId
WHERE t.Status = 'Active' 
  AND t.TransactionType = 'Borrow';
