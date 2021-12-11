using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IdGen;

namespace SITConnect.Models
{
    public class AuditLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public long LogId { get; set; } = new IdGenerator(1).CreateId();

        public long ActorId { get; set; }
        public string LogType { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
    }
}