using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    /// <summary>
    /// Represents a book in the library system
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Primary key for the book
        /// </summary>
        [Key]
        public int BookId { get; set; }

        /// <summary>
        /// Title of the book
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Author of the book
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// International Standard Book Number
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        /// <summary>
        /// Publisher of the book
        /// </summary>
        [StringLength(255)]
        public string? Publisher { get; set; }

        /// <summary>
        /// Year the book was published
        /// </summary>
        [Range(1000, 2100)]
        public int? PublicationYear { get; set; }

        /// <summary>
        /// Genre/category of the book
        /// </summary>
        [StringLength(100)]
        public string? Genre { get; set; }

        /// <summary>
        /// Total number of copies available in the library
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        public int TotalCopies { get; set; } = 1;

        /// <summary>
        /// Number of copies currently available for borrowing
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        public int AvailableCopies { get; set; } = 1;

        /// <summary>
        /// Physical location of the book in the library (shelf number, etc.)
        /// </summary>
        [StringLength(100)]
        public string? Location { get; set; }

        /// <summary>
        /// Date when the book record was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Date when the book record was last updated
        /// </summary>
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Navigation property for transactions related to this book
        /// </summary>
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        /// <summary>
        /// Checks if the book is available for borrowing
        /// </summary>
        /// <returns>True if available copies > 0, false otherwise</returns>
        public bool IsAvailable()
        {
            return AvailableCopies > 0;
        }

        /// <summary>
        /// Decreases available copies when a book is borrowed
        /// </summary>
        /// <returns>True if successful, false if no copies available</returns>
        public bool BorrowCopy()
        {
            if (AvailableCopies > 0)
            {
                AvailableCopies--;
                UpdatedDate = DateTime.Now;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Increases available copies when a book is returned
        /// </summary>
        /// <returns>True if successful, false if already at maximum copies</returns>
        public bool ReturnCopy()
        {
            if (AvailableCopies < TotalCopies)
            {
                AvailableCopies++;
                UpdatedDate = DateTime.Now;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the number of copies currently borrowed
        /// </summary>
        /// <returns>Number of borrowed copies</returns>
        public int BorrowedCopies => TotalCopies - AvailableCopies;

        /// <summary>
        /// String representation of the book
        /// </summary>
        /// <returns>Formatted string with book details</returns>
        public override string ToString()
        {
            return $"{Title} by {Author} (ISBN: {ISBN}) - Available: {AvailableCopies}/{TotalCopies}";
        }
    }
}
