using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pazar.Models
{
    public class AdModel
    {
        [Key]
        public int AdId { get; set; }

        [ForeignKey("ApplicationUser")]
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public virtual CategoryModel Category { get; set; }

        [ForeignKey("Type")]
        public int TypeId { get; set; }
        public virtual TypeModel Type { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public decimal? Price { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public string Location { get; set; }

        public string ImagePath { get; set; }

    }

    public class AdDTO
    {
        public int AdId { get; set; }

        public string ApplicationUserId { get; set; }

        public int CategoryId { get; set; }

        [DisplayName("Listing Category")]
        public string Category { get; set; }

        public int TypeId { get; set; }

        [DisplayName("For Sale / Wanted")]
        public string Type { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal? Price { get; set; }

        [DisplayName("Posted On")]
        public DateTime Timestamp { get; set; }

        public string Location { get; set; }

        public string ImagePath { get; set; }
    }



}