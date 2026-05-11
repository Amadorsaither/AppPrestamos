namespace AppPrestamos.Models
{
    public class Pago
    {
        public int Id { get; set; }

        public int CuotaId { get; set; }
        public Cuota Cuota { get; set; } = null!;

        public DateTime FechaPago { get; set; } = DateTime.Now;

        public decimal MontoPagado { get; set; }

        public string Observacion { get; set; } = string.Empty;
    }
}