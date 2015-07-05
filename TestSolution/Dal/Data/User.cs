using System.Collections.Generic;

namespace Data
{
    public class User
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public virtual ICollection<Bot> Bots { get; set; }
    }
}
