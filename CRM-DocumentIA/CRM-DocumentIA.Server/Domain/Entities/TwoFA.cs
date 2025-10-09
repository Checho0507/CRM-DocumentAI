using System;

namespace CRM_DocumentIA.Server.Domain.Entities
{
    public class TwoFA
    {
        public Guid Id { get; set; }
        public int UsuarioId { get; set; }
        public string CodeHash { get; set; } = default!; // hash del código
        public DateTime ExpiresAt { get; set; }
        public bool Verified { get; set; }
        public int Attempts { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}