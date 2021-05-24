using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pazar.Models.Viewmodels
{
    public class AdCrudModel
    {
        public AdDTO ad { get; set; }
        
        public IEnumerable<TypeDTO> allTypes { get; set; }

        public IEnumerable<CategoryDTO> allCatagories { get; set; }
    }
}