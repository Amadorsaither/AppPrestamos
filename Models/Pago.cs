namespace AppPrestamos.Models
{
    /// <summary>Representa un pago aplicado a una cuota</summary>
    public class Pago
    {
        /// <summary>Identificador único del pago</summary>
        public int Id { get; set; }
        /// <summary>Identificador de la cuota a la que se aplica el pago</summary>
        public int CuotaId { get; set; }
        /// <summary>Cuota a la que se aplica el pago</summary>
        public Cuota Cuota { get; set; } = null!;
        /// <summary>Fecha en que se realizó el pago</summary>
        public DateTime FechaPago { get; set; } = DateTime.Now;
        /// <summary>Monto pagado en esta transacción</summary>
        public decimal MontoPagado { get; set; }
        /// <summary>Observación o nota asociada al pago</summary>
        public string Observacion { get; set; } = string.Empty;
    }
}