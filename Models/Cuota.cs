using AppPrestamos.Enums;

namespace AppPrestamos.Models
{
    public class Cuota
    {
        public int Id { get; set; }

        public int PrestamoId { get; set; }
        public Prestamo Prestamo { get; set; } = null!;

        public int NumeroCuota { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public decimal Capital { get; set; }
        public decimal Interes { get; set; }
        public decimal Mora { get; set; }

        public decimal MontoTotal { get; set; }
        public decimal SaldoPendiente { get; set; }

        public EstadoCuota Estado { get; set; } = EstadoCuota.Pendiente;

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}
