using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    /// <summary>
    /// Represents a borrowing/returning transaction in the library system
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Primary key for the transaction
        /// </summary>
        [Key]
        public int TransactionId { get; set; }

        /// <summary>
        /// Foreign key to the Book being borrowed/returned
        /// </summary>
        [Required]
        public int BookId { get; set; }

        /// <summary>
        /// Foreign key to the Borrower performing the transaction
        /// </summary>
        [Required]
        public int BorrowerId { get; set; }

        /// <summary>
        /// Type of transaction (Borrow, Return, Renew)
        /// </summary>
        [Required]
        public TransactionType TransactionType { get; set; }

        /// <summary>
        /// Date and time when the transaction occurred
        /// </summary>
        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Due date for returning the book (applicable for Borrow transactions)
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Actual return date (applicable for Return transactions)
        /// </summary>
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Fine amount for late returns or damages
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue)]
        public decimal FineAmount { get; set; } = 0.00m;

        /// <summary>
        /// Current status of the transaction
        /// </summary>
        [Required]
        public TransactionStatus Status { get; set; } = TransactionStatus.Active;

        /// <summary>
        /// Additional notes about the transaction
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Date when the transaction record was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Date when the transaction record was last updated
        /// </summary>
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Navigation property to the associated Book
        /// </summary>
        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;

        /// <summary>
        /// Navigation property to the associated Borrower
        /// </summary>
        [ForeignKey("BorrowerId")]
        public virtual Borrower Borrower { get; set; } = null!;

        /// <summary>
        /// Checks if the transaction is overdue
        /// </summary>
        /// <returns>True if overdue, false otherwise</returns>
        public bool IsOverdue()
        {
            return DueDate.HasValue && DateTime.Today > DueDate.Value && Status == TransactionStatus.Active;
        }

        /// <summary>
        /// Gets the number of days overdue
        /// </summary>
        /// <returns>Number of days overdue, 0 if not overdue</returns>
        public int DaysOverdue()
        {
            if (!IsOverdue()) return 0;
            return (DateTime.Today - DueDate!.Value).Days;
        }

        /// <summary>
        /// Calculates fine amount based on days overdue
        /// </summary>
        /// <param name="finePerDay">Fine amount per day</param>
        /// <returns>Calculated fine amount</returns>
        public decimal CalculateFine(decimal finePerDay = 0.50m)
        {
            if (!IsOverdue()) return 0;
            return DaysOverdue() * finePerDay;
        }

        /// <summary>
        /// Marks the transaction as returned
        /// </summary>
        /// <param name="returnDate">Date of return (defaults to current date)</param>
        public void MarkAsReturned(DateTime? returnDate = null)
        {
            ReturnDate = returnDate ?? DateTime.Now;
            Status = TransactionStatus.Completed;
            UpdatedDate = DateTime.Now;
        }

        /// <summary>
        /// Marks the transaction as overdue and calculates fine
        /// </summary>
        /// <param name="finePerDay">Fine amount per day</param>
        public void MarkAsOverdue(decimal finePerDay = 0.50m)
        {
            Status = TransactionStatus.Overdue;
            FineAmount = CalculateFine(finePerDay);
            UpdatedDate = DateTime.Now;
        }

        /// <summary>
        /// Renews the transaction by extending the due date
        /// </summary>
        /// <param name="additionalDays">Number of additional days to extend</param>
        public void Renew(int additionalDays = 14)
        {
            if (DueDate.HasValue)
            {
                DueDate = DueDate.Value.AddDays(additionalDays);
            }
            else
            {
                DueDate = DateTime.Today.AddDays(additionalDays);
            }
            
            TransactionType = TransactionType.Renew;
            UpdatedDate = DateTime.Now;
        }

        /// <summary>
        /// String representation of the transaction
        /// </summary>
        /// <returns>Formatted string with transaction details</returns>
        public override string ToString()
        {
            return $"{TransactionType} - Book ID: {BookId}, Borrower ID: {BorrowerId}, Date: {TransactionDate:yyyy-MM-dd}, Status: {Status}";
        }
    }

    /// <summary>
    /// Enumeration for transaction types
    /// </summary>
    public enum TransactionType
    {
        Borrow,
        Return,
        Renew
    }

    /// <summary>
    /// Enumeration for transaction status
    /// </summary>
    public enum TransactionStatus
    {
        Active,
        Completed,
        Overdue,
        Lost
    }
}
