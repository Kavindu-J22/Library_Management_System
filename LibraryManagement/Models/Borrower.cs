using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    /// <summary>
    /// Represents a library member who can borrow books
    /// </summary>
    public class Borrower
    {
        /// <summary>
        /// Primary key for the borrower
        /// </summary>
        [Key]
        public int BorrowerId { get; set; }

        /// <summary>
        /// First name of the borrower
        /// </summary>
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Last name of the borrower
        /// </summary>
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the borrower (must be unique)
        /// </summary>
        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Phone number of the borrower
        /// </summary>
        [StringLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Physical address of the borrower
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Date when the borrower became a member
        /// </summary>
        [Required]
        public DateTime MembershipDate { get; set; } = DateTime.Today;

        /// <summary>
        /// Type of membership (Student, Faculty, Public, Staff)
        /// </summary>
        [Required]
        public MembershipType MembershipType { get; set; } = MembershipType.Public;

        /// <summary>
        /// Whether the borrower's membership is active
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Maximum number of books this borrower can have at once
        /// </summary>
        [Required]
        [Range(1, 20)]
        public int MaxBooksAllowed { get; set; } = 5;

        /// <summary>
        /// Date when the borrower record was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Date when the borrower record was last updated
        /// </summary>
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Navigation property for transactions related to this borrower
        /// </summary>
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        /// <summary>
        /// Gets the full name of the borrower
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Gets the current number of books borrowed by this borrower
        /// </summary>
        /// <returns>Number of currently borrowed books</returns>
        public int CurrentBorrowedBooks => Transactions?.Count(t => t.Status == TransactionStatus.Active && t.TransactionType == TransactionType.Borrow) ?? 0;

        /// <summary>
        /// Checks if the borrower can borrow more books
        /// </summary>
        /// <returns>True if can borrow more books, false otherwise</returns>
        public bool CanBorrowMoreBooks()
        {
            return IsActive && CurrentBorrowedBooks < MaxBooksAllowed;
        }

        /// <summary>
        /// Gets the number of additional books this borrower can borrow
        /// </summary>
        /// <returns>Number of additional books that can be borrowed</returns>
        public int RemainingBorrowLimit => Math.Max(0, MaxBooksAllowed - CurrentBorrowedBooks);

        /// <summary>
        /// Checks if the borrower has any overdue books
        /// </summary>
        /// <returns>True if has overdue books, false otherwise</returns>
        public bool HasOverdueBooks()
        {
            return Transactions?.Any(t => t.Status == TransactionStatus.Overdue) ?? false;
        }

        /// <summary>
        /// Gets the total fine amount for this borrower
        /// </summary>
        /// <returns>Total fine amount</returns>
        public decimal TotalFineAmount => Transactions?.Where(t => t.FineAmount > 0).Sum(t => t.FineAmount) ?? 0;

        /// <summary>
        /// String representation of the borrower
        /// </summary>
        /// <returns>Formatted string with borrower details</returns>
        public override string ToString()
        {
            return $"{FullName} ({Email}) - {MembershipType} - Books: {CurrentBorrowedBooks}/{MaxBooksAllowed}";
        }
    }

    /// <summary>
    /// Enumeration for membership types
    /// </summary>
    public enum MembershipType
    {
        Student,
        Faculty,
        Public,
        Staff
    }
}
