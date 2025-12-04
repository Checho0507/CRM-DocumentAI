namespace CRM_DocumentIA.Server.Application.DTOs.Analytics
{
public class DashboardSummaryDto
{
    public int TotalUsuarios { get; set; }
    public int TotalChats { get; set; }
    public int TotalConsultas { get; set; }
    public int UsuariosActivosUltimos30Dias { get; set; }
}
}