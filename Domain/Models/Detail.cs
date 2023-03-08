using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Detail
    {
        [Key]
        [Required]
        public int Id { get; set; }
        public Document Document { get; set; }
        public Definition Definition { get; set; }
        public Metadata Parent { get; set; }
        public Metadata Metadata { get; set; }

        public int ParamId { get; set; }
        public int? ParamParentId { get; set; }

        public int? PageIndex { get; set; }
        public int? ValueIndex { get; set; }
        public int? RowIndex { get; set; }
        public int? ColIndex { get; set; }

        public string Value { get; set; }

    }
}
