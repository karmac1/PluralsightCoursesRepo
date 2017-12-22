using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public class BookUpdateDto : BookGeneralDto
    {
        [Required(ErrorMessage = "You should fill out a description.")]
        public override string Description
        {
            get { return base.Description; }
            set { value = base.Description; }
        }
    }
}
 