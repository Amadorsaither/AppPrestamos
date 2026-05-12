namespace AppPrestamos.Models
{
    /// <summary>Representa un cliente registrado en el sistema</summary>
    public class Cliente
    {
        /// <summary>Identificador único del cliente</summary>
        public int Id { get; set; }
        /// <summary>Nombre completo del cliente</summary>
        public string Nombre { get; set; } = string.Empty;
        /// <summary>Cédula de identidad del cliente</summary>
        public string Cedula { get; set; } = string.Empty;
        /// <summary>Número de teléfono del cliente</summary>
        public string Telefono { get; set; } = string.Empty;
        /// <summary>Dirección física del cliente</summary>
        public string Direccion { get; set; } = string.Empty;
        /// <summary>Fecha en que se registró el cliente en el sistema</summary>
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        /// <summary>Colección de préstamos asociados al cliente</summary>
        public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    }
}