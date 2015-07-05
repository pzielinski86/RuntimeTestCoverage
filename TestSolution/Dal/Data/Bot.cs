using System.ComponentModel.DataAnnotations;

namespace Data
{
    public class Bot
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public User User { get; set; }
        [Required]
        public string Name { get; set; }
        public byte[] DllData { get; set; }
    }
}
