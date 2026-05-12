using AppPrestamos.Enums;

namespace AppPrestamos.Models
{
    /// <summary>Representa un préstamo otorgado a un cliente</summary>
    public class Prestamo
    {
        /// <summary>Identificador único del préstamo</summary>
        public int Id { get; set; }
        /// <summary>Identificador del cliente asociado al préstamo</summary>
        public int ClienteId { get; set; }
        /// <summary>Cliente al que se le otorgó el préstamo</summary>
        public Cliente Cliente { get; set; } = null!;
        /// <summary>Monto total del préstamo</summary>
        public decimal Monto { get; set; }
        /// <summary>Tasa de interés del préstamo</summary>
        public decimal TasaInteres { get; set; }
        /// <summary>Tipo de interés aplicado (simple o compuesto)</summary>
        public TipoInteres TipoInteres { get; set; }
        /// <summary>Frecuencia de pago de las cuotas</summary>
        public FrecuenciaPago FrecuenciaPago { get; set; }
        /// <summary>Número total de cuotas del préstamo</summary>
        public int NumeroCuotas { get; set; }
        /// <summary>Fecha de inicio del préstamo</summary>
        public DateTime FechaInicio { get; set; } = DateTime.Now;
        /// <summary>Tasa de mora diaria por incumplimiento</summary>
        public decimal TasaMoraDiaria { get; set; }
        /// <summary>Estado actual del préstamo</summary>
        public EstadoPrestamo Estado { get; set; } = EstadoPrestamo.Activo;
        /// <summary>Colección de cuotas generadas para este préstamo</summary>
        public ICollection<Cuota> Cuotas { get; set; } = new List<Cuota>();
    }
}