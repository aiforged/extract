using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Definition
    {
        [Key]
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
        public Domain.Enum.Enums.ValueType DataType { get; set; }
        public AIForged.API.ParameterDefinitionCategory Category { get; set; }
        public AIForged.API.GroupingType? Grouping { get; set; }
        public int? Index { get; set; }
        public int ExternalId { get; set; }
        public int? ExternalParentId { get; set; }
        public Definition Parent { get; set; }

    }
}
