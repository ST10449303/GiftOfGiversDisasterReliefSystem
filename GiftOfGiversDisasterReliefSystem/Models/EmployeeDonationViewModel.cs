using System;

namespace GiftOfGiversDisasterReliefSystem.Models
{
    public class EmployeeDonationViewModel
    {
        public int Id { get; set; }

        public string DonorName { get; set; } = "Anonymous Donor";

        public string DonorEmail { get; set; } = "-";

        public decimal Amount { get; set; }

        public string DonationType { get; set; } = "";

        public string Currency { get; set; } = "";

        public bool IsAnonymous { get; set; }

        public DateTime DonationDate { get; set; }

        public string? TaxCertificateNumber { get; set; }
    }
}