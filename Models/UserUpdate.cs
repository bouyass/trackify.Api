using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Trackify.Api.Models
{
    public class UserUpdate
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // L'utilisateur concerné
        public Guid UserId { get; set; }
        public User? User { get; set; }

        // L'update (nouveau contenu) concerné
        public Guid UpdateId { get; set; }
        public Update? Update { get; set; }

        // État : est-ce que l'utilisateur l'a déjà consulté ?
        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; } // date de lecture si IsRead = true
    }

}
