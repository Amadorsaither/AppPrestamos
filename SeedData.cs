using AppPrestamos.Data;
using AppPrestamos.Enums;
using AppPrestamos.Models;
using AppPrestamos.Services;

namespace AppPrestamos;

/// <summary>Genera datos de prueba para la aplicaci�n</summary>
public static class SeedData
{
    private static readonly string[] Nombres =
    [
        "Juan P�rez", "Mar�a Rodr�guez", "Carlos Mart�nez", "Ana Garc�a",
        "Luis Santos", "Rosa Castillo", "Jos� G�mez", "Carmen Reyes",
        "Pedro Ram�rez", "Marta Jim�nez", "Rafael Herrera", "Diana M�ndez",
        "Miguel Cruz", "Laura Vargas", "Fernando Ortiz", "Sandra Torres",
        "Antonio Medina", "Patricia Romero", "Francisco Moreno", "Teresa Silva",
        "Manuel Guzm�n", "Elena C�rdova", "H�ctor Polanco", "Yolanda Pe�a",
        "Alberto Rivas", "Julia Feliz", "Ricardo Guerrero", "Sonia Mej�a",
        "Jorge Beltr�", "Claudia Morillo"
    ];

    private static readonly string[] Apellidos = [];

    private static int _index;

    private static string ProxNombre() => Nombres[_index++ % Nombres.Length];

    private static string Cedula()
    {
        var rand = Random.Shared;
        var dig = new int[11];
        dig[0] = rand.Next(1, 9);
        dig[1] = rand.Next(0, 9);
        dig[2] = rand.Next(0, 9);
        dig[3] = rand.Next(0, 9);
        dig[4] = rand.Next(0, 9);
        dig[5] = rand.Next(0, 9);
        dig[6] = rand.Next(0, 9);
        dig[7] = rand.Next(0, 9);
        dig[8] = rand.Next(0, 9);
        var suma = 0;
        var peso = 1;
        for (int i = 0; i < 9; i++)
        {
            var mul = dig[i] * peso;
            suma += mul > 9 ? mul - 9 : mul;
            peso = peso == 1 ? 2 : 1;
        }
        var decena = (suma / 10 + 1) * 10;
        dig[9] = decena - suma;
        if (dig[9] == 10) dig[9] = 0;
        dig[10] = 1;
        return string.Concat(dig);
    }

    private static string Telefono()
    {
        var pref = Random.Shared.Next(0, 3) switch { 0 => "809", 1 => "829", _ => "849" };
        return $"{pref}-{Random.Shared.Next(100, 999)}-{Random.Shared.Next(1000, 9999)}";
    }

    private static string Direccion()
    {
        var calles = new[] { "Calle Principal", "Av. Independencia", "Calle Duarte", "Av. M�xima G�mez", "Calle Espa�a", "Av. 27 de Febrero", "Calle El Sol", "Av. Abraham Lincoln", "Calle Las Flores", "Av. Winston Churchill" };
        return $"{calles[Random.Shared.Next(calles.Length)]} #{Random.Shared.Next(1, 200)}, Santo Domingo";
    }

    /// <summary>Puebla la base de datos con 30 clientes y pr�stamos de ejemplo si est� vac�a</summary>
    public static void Seed()
    {
        using var db = new AppDbContext();
        if (db.Clientes.Any()) return;

        _index = 0;
        var service = new PrestamoService();
        var hoy = DateTime.Today;

        // Crear 30 clientes
        var clientes = new List<Cliente>();
        for (int i = 0; i < 30; i++)
        {
            var c = new Cliente
            {
                Nombre = ProxNombre(),
                Cedula = Cedula(),
                Telefono = Telefono(),
                Direccion = Direccion(),
                FechaRegistro = hoy.AddDays(-Random.Shared.Next(1, 180))
            };
            db.Clientes.Add(c);
            clientes.Add(c);
        }
        db.SaveChanges();

        // Asignar pr�stamos a cada cliente
        for (int i = 0; i < clientes.Count; i++)
        {
            var cliente = clientes[i];
            int numPrestamos = Random.Shared.Next(1, 4); // 1-3 pr�stamos por cliente

            for (int p = 0; p < numPrestamos; p++)
            {
                var estado = AsignarEstado(i, p);
                var monto = Random.Shared.Next(5, 100) * 1000m; // 5,000 - 100,000
                var tasaInteres = Random.Shared.Next(1, 5); // 1% - 5%
                var numCuotas = Random.Shared.Next(3, 13); // 3-12 cuotas
                var frecuencia = (FrecuenciaPago)Random.Shared.Next(0, 3);
                var fechaInicio = estado == EstadoPrestamo.Pagado
                    ? hoy.AddMonths(-Random.Shared.Next(numCuotas + 1, numCuotas + 6))
                    : estado == EstadoPrestamo.EnMora
                        ? hoy.AddMonths(-Random.Shared.Next(2, numCuotas + 2))
                        : hoy.AddMonths(-Random.Shared.Next(0, 3));

                var prestamo = new Prestamo
                {
                    ClienteId = cliente.Id,
                    Monto = monto,
                    TasaInteres = tasaInteres,
                    TipoInteres = (TipoInteres)Random.Shared.Next(0, 2),
                    FrecuenciaPago = frecuencia,
                    NumeroCuotas = numCuotas,
                    FechaInicio = fechaInicio,
                    TasaMoraDiaria = 0.1m,
                    Estado = estado
                };
                db.Prestamos.Add(prestamo);
                db.SaveChanges();

                // Generar cuotas
                var cuotas = service.GenerarCuotas(prestamo);
                foreach (var cuota in cuotas)
                {
                    cuota.PrestamoId = prestamo.Id;
                    db.Cuotas.Add(cuota);
                }
                db.SaveChanges();

                // Marcar cuotas seg�n estado del pr�stamo
                var cuotasDb = db.Cuotas.Where(c => c.PrestamoId == prestamo.Id).OrderBy(c => c.NumeroCuota).ToList();
                switch (estado)
                {
                    case EstadoPrestamo.Pagado:
                        // Todas pagadas
                        for (int ci = 0; ci < cuotasDb.Count; ci++)
                        {
                            var c = cuotasDb[ci];
                            c.Estado = EstadoCuota.Pagada;
                            c.SaldoPendiente = 0;
                            // Registrar pago
                            db.Pagos.Add(new Pago
                            {
                                CuotaId = c.Id,
                                FechaPago = c.FechaVencimiento.AddDays(-Random.Shared.Next(0, 3)),
                                MontoPagado = c.MontoTotal,
                                Observacion = "Pago realizado (seed data)"
                            });
                        }
                        break;

                    case EstadoPrestamo.EnMora:
                        // Primeras cuotas vencidas sin pagar
                        int vencidas = Random.Shared.Next(1, Math.Min(cuotasDb.Count, 4));
                        for (int ci = 0; ci < cuotasDb.Count; ci++)
                        {
                            var c = cuotasDb[ci];
                            if (ci < vencidas)
                            {
                                c.Estado = EstadoCuota.Vencida;
                                c.FechaVencimiento = c.FechaVencimiento.AddMonths(-Random.Shared.Next(1, 3));
                            }
                            else
                            {
                                c.Estado = EstadoCuota.Pendiente;
                            }
                        }
                        break;

                    case EstadoPrestamo.Activo:
                        // Algunas pagadas, resto pendiente
                        int pagadas = Random.Shared.Next(0, Math.Max(cuotasDb.Count / 2, 1));
                        for (int ci = 0; ci < cuotasDb.Count; ci++)
                        {
                            var c = cuotasDb[ci];
                            if (ci < pagadas)
                            {
                                c.Estado = EstadoCuota.Pagada;
                                c.SaldoPendiente = 0;
                                db.Pagos.Add(new Pago
                                {
                                    CuotaId = c.Id,
                                    FechaPago = c.FechaVencimiento.AddDays(-Random.Shared.Next(0, 2)),
                                    MontoPagado = c.MontoTotal,
                                    Observacion = "Pago parcial/seed"
                                });
                            }
                        }
                        break;
                }
            }
        }
        db.SaveChanges();
    }

    private static EstadoPrestamo AsignarEstado(int clienteIdx, int prestamoIdx)
    {
        var r = Random.Shared.Next(0, 10);
        return r switch
        {
            < 3 => EstadoPrestamo.Pagado,
            < 7 => EstadoPrestamo.Activo,
            _ => EstadoPrestamo.EnMora
        };
    }
}
