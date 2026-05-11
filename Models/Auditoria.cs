namespace AppPrestamos.Models
{
    public class Auditoria
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = "";
        public string Accion { get; set; } = "";
        public string Entidad { get; set; } = "";
        public int EntidadId { get; set; }
        public string Detalle { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
