using AppPrestamos.Enums;

namespace AppPrestamos.Models
{
    public class Prestamo
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public decimal Monto { get; set; }
        public decimal TasaInteres { get; set; }

        public TipoInteres TipoInteres { get; set; }
        public FrecuenciaPago FrecuenciaPago { get; set; }

        public int NumeroCuotas { get; set; }
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        public decimal TasaMoraDiaria { get; set; }

        public EstadoPrestamo Estado { get; set; } = EstadoPrestamo.Activo;

        public ICollection<Cuota> Cuotas { get; set; } = new List<Cuota>();
    }
}