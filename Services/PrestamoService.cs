using AppPrestamos.Enums;
using AppPrestamos.Models;

namespace AppPrestamos.Services
{
    /// <summary>Servicio de cálculo y generación de préstamos y cuotas</summary>
    public class PrestamoService
    {
        /// <summary>Calcula el monto total del préstamo según el tipo de interés (simple o compuesto)</summary>
        public decimal CalcularMontoTotal(
            decimal monto,
            decimal tasaInteres,
            int numeroCuotas,
            TipoInteres tipoInteres)
        {
            decimal tasa = tasaInteres / 100m;

            if (tipoInteres == TipoInteres.Simple)
            {
                decimal interes = monto * tasa;
                return monto + interes;
            }
            else
            {
                return monto * (decimal)Math.Pow((double)(1 + tasa), numeroCuotas);
            }
        }

        /// <summary>Genera la lista de cuotas para un préstamo distribuyendo capital e intereses</summary>
        public List<Cuota> GenerarCuotas(Prestamo prestamo)
        {
            var cuotas = new List<Cuota>();

            decimal montoTotal = CalcularMontoTotal(
                prestamo.Monto,
                prestamo.TasaInteres,
                prestamo.NumeroCuotas,
                prestamo.TipoInteres);

            decimal valorCuota = Math.Round(
                montoTotal / prestamo.NumeroCuotas,
                2);

            for (int i = 1; i <= prestamo.NumeroCuotas; i++)
            {
                DateTime fechaVencimiento = CalcularFechaVencimiento(
                    prestamo.FechaInicio,
                    prestamo.FrecuenciaPago,
                    i);

                var cuota = new Cuota
                {
                    NumeroCuota = i,
                    FechaVencimiento = fechaVencimiento,
                    Capital = prestamo.Monto / prestamo.NumeroCuotas,
                    Interes = (montoTotal - prestamo.Monto) / prestamo.NumeroCuotas,
                    Mora = 0,
                    MontoTotal = valorCuota,
                    SaldoPendiente = valorCuota,
                    Estado = EstadoCuota.Pendiente
                };

                cuotas.Add(cuota);
            }

            return cuotas;
        }

        /// <summary>Calcula la fecha de vencimiento de una cuota según la frecuencia de pago</summary>
        public DateTime CalcularFechaVencimiento(
            DateTime fechaInicio,
            FrecuenciaPago frecuencia,
            int numeroCuota)
        {
            return frecuencia switch
            {
                FrecuenciaPago.Semanal =>
                    fechaInicio.AddDays(7 * numeroCuota),

                FrecuenciaPago.Quincenal =>
                    fechaInicio.AddDays(15 * numeroCuota),

                FrecuenciaPago.Mensual =>
                    fechaInicio.AddMonths(numeroCuota),

                _ => fechaInicio
            };
        }
    }
}