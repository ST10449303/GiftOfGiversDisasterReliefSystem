using System;
using System.ComponentModel.DataAnnotations;

namespace GiftOfGiversDisasterReliefSystem.Data
{
    public class ReliefUpdate
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        public DateTime DatePosted { get; set; }

        [StringLength(150)]
        public string PostedBy { get; set; } = string.Empty;
    }
}