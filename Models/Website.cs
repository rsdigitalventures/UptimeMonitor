using System;
using System.ComponentModel.DataAnnotations;

namespace UptimeMonitor.Models
{
    public class Website
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Url { get; set; }

        public bool Enabled { get; set; }
    }

    public class UptimeRecord
    {
        [Key]
        public long Id { get; set; }

        public long WebsiteId { get; set; }

        public Website Website { get; set; }

        public bool Online { get; set; }

        public DateTime Date { get; set; }
    }
}
