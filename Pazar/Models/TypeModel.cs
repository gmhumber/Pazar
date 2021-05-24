using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pazar.Models
{
    public class TypeModel
    {
        [Key]
        public int TypeId { get; set; }

        [Required]
        public string Type { get; set; }

        // Establish a 1-to-many relationship with the Ads table
        public ICollection<AdModel> Ads { get; set; }
    }

    public class TypeDTO
    {
        public int TypeId { get; set; }

        public string Type { get; set; }
    }

}