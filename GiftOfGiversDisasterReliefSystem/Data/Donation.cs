using System;
using System.ComponentModel.DataAnnotations;

namespace GiftOfGiversDisasterReliefSystem.Data
{
    public class Donation
    {
        [Key]
        public int Id { get; set; }

        // The donor's Identity user ID.
        // Null for anonymous public donations.
        public string? UserId { get; set; }

        // Donation amount
        [Required]
        public decimal Amount { get; set; }

        // One-Time or Recurring
        [Required]
        [MaxLength(50)]
        public string DonationType { get; set; } = string.Empty;

        // ZAR, USD or EUR
        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = string.Empty;

        // True when the donation was made anonymously
        public bool IsAnonymous { get; set; }

        // Date and time of donation
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;

        // Tax certificate number
        [MaxLength(100)]
        public string? TaxCertificateNumber { get; set; }
    }
}