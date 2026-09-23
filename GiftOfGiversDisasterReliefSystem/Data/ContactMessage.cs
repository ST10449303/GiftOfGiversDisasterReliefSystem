using System;
using System.ComponentModel.DataAnnotations;

namespace GiftOfGiversDisasterReliefSystem.Data
{
    public class ContactMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime DateSent { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}