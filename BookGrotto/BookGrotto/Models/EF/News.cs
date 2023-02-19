using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace BookGrotto.Models.EF
{
    [Table("tb_New")]
    public class News : CommonAbstract
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(150)]
        public string Title {get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public int CategoryId { get; set; }
        public string SeoTitle { get; set; }
        public string SeoDescription { get; set;}
        public string SeoKeywords { get; set; }
        public virtual Category Category { get; set; }
    }
}