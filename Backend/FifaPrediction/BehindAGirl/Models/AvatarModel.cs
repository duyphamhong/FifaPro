using System.ComponentModel.DataAnnotations;

namespace BehindAGirl.Models
{
    public class AvatarModel
    {
        [Required]
        public string AvatarUrl { get; set; }
    }
}
