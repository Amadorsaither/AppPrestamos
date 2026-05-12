using AppPrestamos.Enums;

namespace AppPrestamos.Models
{
    /// <summary>Representa una cuota dentro de un préstamo</summary>
    public class Cuota
    {
        /// <summary>Identificador único de la cuota</summary>
        public int Id { get; set; }
        /// <summary>Identificador del préstamo al que pertenece esta cuota</summary>
        public int PrestamoId { get; set; }
        /// <summary>Préstamo al que pertenece esta cuota</summary>
        public Prestamo Prestamo { get; set; } = null!;
        /// <summary>Número de la cuota dentro del plan de pagos</summary>
        public int NumeroCuota { get; set; }
        /// <summary>Fecha de vencimiento de la cuota</summary>
        public DateTime FechaVencimiento { get; set; }
        /// <summary>Monto de capital de la cuota</summary>
        public decimal Capital { get; set; }
        /// <summary>Monto de interés de la cuota</summary>
        public decimal Interes { get; set; }
        /// <summary>Monto de mora aplicado a la cuota</summary>
        public decimal Mora { get; set; }
        /// <summary>Monto total de la cuota (capital + interés + mora)</summary>
        public decimal MontoTotal { get; set; }
        /// <summary>Saldo pendiente por pagar de la cuota</summary>
        public decimal SaldoPendiente { get; set; }
        /// <summary>Estado actual de la cuota</summary>
        public EstadoCuota Estado { get; set; } = EstadoCuota.Pendiente;
        /// <summary>Colección de pagos realizados sobre esta cuota</summary>
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}
