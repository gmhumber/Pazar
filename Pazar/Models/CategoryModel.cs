using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pazar.Models
{
    public class CategoryModel
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string Category { get; set; }

        // Establish a 1-to-many relationship with the Ads table
        public ICollection<AdModel> Ads { get; set; }

    }

    
    public class CategoryDTO
    {
        public int CategoryId { get; set; }

        public string Category { get; set; }

    }

}