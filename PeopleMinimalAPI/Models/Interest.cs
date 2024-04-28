using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PeopleMinimalAPI.Models
{
    public class Interest
    {
        [Key]
        public int InterestId { get; set; }
        [StringLength(35)]
        public string Title { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
        [ForeignKey("Person")]
        public int FkPersonId { get; set; }
        public Person Person { get; set; }
        [JsonIgnore]
        public ICollection<Link>? Links { get; set; } = new List<Link>();
    }
}
