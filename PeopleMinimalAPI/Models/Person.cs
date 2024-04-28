using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PeopleMinimalAPI.Models
{
    public class Person
    {
        [Key]
        public int PersonId { get; set; }
        [StringLength(60)]
        public string Name { get; set; }
        [DataType(DataType.PhoneNumber)]
        [StringLength(15)]
        public string PhoneNumber { get; set; }
        [JsonIgnore]
        public ICollection<Interest> Interests { get; set; } = new List<Interest>();
        
    }
}
