namespace AppPrestamos.Models
{
    /// <summary>Representa un usuario del sistema</summary>
    public class Usuario
    {
        /// <summary>Identificador único del usuario</summary>
        public int Id { get; set; }
        /// <summary>Nombre de inicio de sesión del usuario</summary>
        public string NombreUsuario { get; set; } = string.Empty;
        /// <summary>Hash de la contraseña del usuario</summary>
        public string ContrasenaHash { get; set; } = string.Empty;
        /// <summary>Rol o nivel de acceso del usuario</summary>
        public string Rol { get; set; } = "Usuario";
        /// <summary>Indica si el usuario está activo en el sistema</summary>
        public bool Activo { get; set; } = true;
        /// <summary>Fecha de creación del usuario</summary>
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
