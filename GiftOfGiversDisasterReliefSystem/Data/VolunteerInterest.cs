using System.ComponentModel.DataAnnotations;

namespace GiftOfGiversDisasterReliefSystem.Data
{
    public class VolunteerInterest
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Skills { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Availability { get; set; } = string.Empty;

        public DateTime DateRegistered { get; set; }

        // Volunteer application status
        public string Status { get; set; } = "Waiting";
    }
}