namespace CRM_DocumentIA.Server.Application.DTOs.Analytics
{
    public class TopUsuariosDto
    {
        public int UserId { get; set; }
        public string Nombre { get; set; }
        public int TotalConsultas { get; set; }
    }
}
