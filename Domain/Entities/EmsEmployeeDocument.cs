using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EMS.Domain.Enums;

namespace EMS.Domain.Entities
{
    [Table("EMS_EmployeeDocuments")]
    public class EmsEmployeeDocument
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public EmsDocumentType DocumentType { get; set; }

        [Required]
        [MaxLength(250)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [NotMapped]
        public long FileSizeBytes { get; set; }

        public DateTime UploadedAt { get; set; }

        // Navigation Property
        [ForeignKey("EmployeeId")]
        public virtual EmsEmployee? Employee { get; set; }
    }
}
