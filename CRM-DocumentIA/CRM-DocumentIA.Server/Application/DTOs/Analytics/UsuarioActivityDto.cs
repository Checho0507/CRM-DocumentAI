namespace CRM_DocumentIA.Server.Application.DTOs.Analytics
{
    public class UsuarioActivityDto
    {
        public int UserId { get; set; }
        public string Nombre { get; set; }
        public int TotalChats { get; set; }
        public int TotalInsights { get; set; }
        public DateTime UltimaActividad { get; set; }
    }
}
