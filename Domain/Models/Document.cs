using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Document
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public int ServiceId { get; set; }
        public int DocumentId { get; set; }
        public int? MasterId { get; set; }

        public AIForged.API.UsageType Usage { get; set; }
        public AIForged.API.DocumentStatus Status { get; set; }

        [MaxLength(512)]
        public string Filename { get; set; }
        public string ContentType { get; set; }

        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }

        public DateTime DTC { get; set; }
        public DateTime DTM { get; set; }

        public string Result { get; set; }
        public string ResultId { get; set; }//so we can use Guids...
        public int? ResultIndex { get; set; }//page index of resulting page???
        public string ExternalId { get; set; }
        public string Comment { get; set; }//temp to use for conversion


    }
}
