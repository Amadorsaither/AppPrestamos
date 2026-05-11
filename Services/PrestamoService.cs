using AppPrestamos.Enums;
using AppPrestamos.Models;

namespace AppPrestamos.Services
{
    public class PrestamoService
    {
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