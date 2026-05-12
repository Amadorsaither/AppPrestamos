using AppPrestamos.Data;
using AppPrestamos.Models;

namespace AppPrestamos.Services
{
    /// <summary>Servicio de auditoría para registrar acciones del sistema en la base de datos</summary>
    public class AuditService
    {
        /// <summary>Registra una acción de auditoría con detalles del usuario, entidad y fecha</summary>
        public void Registrar(string accion, string entidad, int entidadId, string detalle)
        {
            using var db = new AppDbContext();
            db.Auditorias.Add(new Auditoria
            {
                UsuarioId = App.UsuarioActual?.Id,
                UsuarioNombre = App.UsuarioActual?.NombreUsuario ?? "Sistema",
                Accion = accion,
                Entidad = entidad,
                EntidadId = entidadId,
                Detalle = detalle,
                Fecha = DateTime.Now
            });
            db.SaveChanges();
        }
    }
}
