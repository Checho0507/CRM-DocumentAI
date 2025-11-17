using CRM_DocumentIA.Server.Domain.Entities;

public class Rol
{
    public Rol(string nombre, string descripcion)
    {
        Nombre = nombre;
        Descripcion = descripcion;
    }

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

}