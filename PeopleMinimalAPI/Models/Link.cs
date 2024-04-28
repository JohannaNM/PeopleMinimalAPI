using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeopleMinimalAPI.Models
{
    public class Link
    {
        [Key]
        public int LinkId { get; set; }
        [StringLength(100)]
        public string Url { get; set; }
        [ForeignKey("Interest")]
        public int FkInterestId { get; set; }
        public Interest Interest { get; set; }
    }
}
