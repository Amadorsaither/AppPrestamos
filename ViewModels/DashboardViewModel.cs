using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using AppPrestamos.Data;
using AppPrestamos.Enums;
using Microsoft.EntityFrameworkCore;

namespace AppPrestamos.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private string totalClientes = "0";

        [ObservableProperty]
        private string subtituloClientes = "Clientes Registrados";

        [ObservableProperty]
        private string incrementoClientes = "";

        [ObservableProperty]
        private string totalPrestamos = "0";

        [ObservableProperty]
        private string subtituloPrestamos = "Pr\u00e9stamos Activos";

        [ObservableProperty]
        private string incrementoPrestamos = "";

        [ObservableProperty]
        private string totalMonto = "$0";

        [ObservableProperty]
        private string subtituloMonto = "Monto Total Prestado";

        [ObservableProperty]
        private string incrementoMonto = "";

        [ObservableProperty]
        private string totalPagos = "$0";

        [ObservableProperty]
        private string subtituloPagos = "Pagos del Mes";

        [ObservableProperty]
        private int activosCount;

        [ObservableProperty]
        private int pagadosCount;

        [ObservableProperty]
        private int moraCount;

        [ObservableProperty]
        private string incrementoPagos = "";

        public ISeries[] SparklineClientes { get; private set; } = [];
        public ISeries[] SparklinePrestamos { get; private set; } = [];
        public ISeries[] SparklineMonto { get; private set; } = [];
        public ISeries[] SparklinePagos { get; private set; } = [];
        public Axis[] EjeVacio { get; } =
        [
            new() { IsVisible = false, LabelsPaint = null, SeparatorsPaint = null },
            new() { IsVisible = false, LabelsPaint = null, SeparatorsPaint = null }
        ];

        public ISeries[] SeriesEstados { get; private set; } = [];
        public ISeries[] SeriesFrecuencia { get; private set; } = [];
        public Axis[] EjesFrecuenciaX { get; private set; } = [];
        public Axis[] EjesFrecuenciaY { get; private set; } = [];

        public ObservableCollection<ProximoVencimiento> Vencimientos { get; } = [];
        public ObservableCollection<PrestamoReciente> PrestamosRecientes { get; } = [];

        [RelayCommand]
        private void NuevoCliente() =>
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Clientes"));

        [RelayCommand]
        private void NuevoPrestamo() =>
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Prestamos"));

        [RelayCommand]
        private void RegistrarPago() =>
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Pagos"));

        [RelayCommand]
        private void ReporteMora() =>
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Reportes"));

        public DashboardViewModel()
        {
            CargarDatos();
        }

        public void CargarDatos()
        {
            using var db = new AppDbContext();

            var totalClientesDb = db.Clientes.Count();
            TotalClientes = totalClientesDb.ToString("N0");
            IncrementoClientes = $"{totalClientesDb} registrados";

            var prestamosActivos = db.Prestamos.Count(p => p.Estado == EstadoPrestamo.Activo);
            TotalPrestamos = prestamosActivos.ToString("N0");
            IncrementoPrestamos = $"{prestamosActivos} en curso";

            var montoTotal = db.Prestamos.AsEnumerable().Sum(p => p.Monto);
            TotalMonto = $"${montoTotal:N2}";
            IncrementoMonto = $"${montoTotal:N0} desembolsados";

            var hoy = DateTime.Today;
            var pagosMes = db.Pagos
                .Where(p => p.FechaPago.Year == hoy.Year && p.FechaPago.Month == hoy.Month)
                .AsEnumerable()
                .Sum(p => p.MontoPagado);
            TotalPagos = $"${pagosMes:N2}";
            IncrementoPagos = $"${pagosMes:N0} recaudados";

            var azul = SKColor.Parse("#3B82F6");
            var verde = SKColor.Parse("#10B981");
            var naranja = SKColor.Parse("#F59E0B");
            var morado = SKColor.Parse("#8B5CF6");
            var rojo = SKColor.Parse("#EF4444");

            SparklineClientes = CrearSparkline(ObtenerTendenciaClientes(db), azul);
            SparklinePrestamos = CrearSparkline(ObtenerTendenciaPrestamos(db), verde);
            SparklineMonto = CrearSparkline(ObtenerTendenciaMonto(db), naranja);
            SparklinePagos = CrearSparkline(ObtenerTendenciaPagos(db), morado);

            ActivosCount = db.Prestamos.Count(p => p.Estado == EstadoPrestamo.Activo);
            PagadosCount = db.Prestamos.Count(p => p.Estado == EstadoPrestamo.Pagado);
            MoraCount = db.Prestamos.Count(p => p.Estado == EstadoPrestamo.EnMora);

            var activos = ActivosCount;
            var pagados = PagadosCount;
            var mora = MoraCount;

            SeriesEstados = new ISeries[]
            {
                new PieSeries<double> { Values = [activos], Name = "Activos", Fill = new SolidColorPaint(verde),
                    HoverPushout = 5, InnerRadius = 60,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#1E293B")), DataLabelsSize = 12 },
                new PieSeries<double> { Values = [pagados], Name = "Pagados", Fill = new SolidColorPaint(azul),
                    InnerRadius = 60,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#1E293B")), DataLabelsSize = 12 },
                new PieSeries<double> { Values = [mora], Name = "En Mora", Fill = new SolidColorPaint(rojo),
                    InnerRadius = 60,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#1E293B")), DataLabelsSize = 12 }
            };

            var semanal = db.Prestamos.Count(p => p.FrecuenciaPago == FrecuenciaPago.Semanal);
            var quincenal = db.Prestamos.Count(p => p.FrecuenciaPago == FrecuenciaPago.Quincenal);
            var mensual = db.Prestamos.Count(p => p.FrecuenciaPago == FrecuenciaPago.Mensual);

            SeriesFrecuencia = new ISeries[]
            {
                new ColumnSeries<double> { Values = [semanal, quincenal, mensual],
                    Fill = new SolidColorPaint(azul), Stroke = null, MaxBarWidth = 50 }
            };
            EjesFrecuenciaX = new Axis[]
            {
                new() { Labels = new[] { "Semanal", "Quincenal", "Mensual" },
                    LabelsRotation = 0, LabelsPaint = new SolidColorPaint(SKColor.Parse("#334155")), TextSize = 12 }
            };
            EjesFrecuenciaY = new Axis[]
            {
                new() { LabelsPaint = null, SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E2E8F0"), 1) }
            };

            Vencimientos.Clear();
            var proximos = db.Cuotas
                .Include(c => c.Prestamo).ThenInclude(p => p.Cliente)
                .Where(c => c.Estado == EstadoCuota.Pendiente || c.Estado == EstadoCuota.Parcial)
                .OrderBy(c => c.FechaVencimiento)
                .Take(5)
                .ToList();

            foreach (var c in proximos)
            {
                var dias = (c.FechaVencimiento - hoy).Days;
                string estado;
                string color;
                if (dias < 0)
                {
                    estado = "Vencida";
                    color = "#EF4444";
                }
                else if (dias == 0)
                {
                    estado = "Vence Hoy";
                    color = "#EF4444";
                }
                else if (dias <= 3)
                {
                    estado = "Pr\u00f3ximo";
                    color = "#F59E0B";
                }
                else
                {
                    estado = "Por Vencer";
                    color = "#10B981";
                }

                Vencimientos.Add(new ProximoVencimiento
                {
                    Cliente = c.Prestamo?.Cliente?.Nombre ?? "",
                    Monto = $"${c.SaldoPendiente:N2}",
                    Fecha = c.FechaVencimiento.ToString("dd/MM/yyyy"),
                    Estado = estado,
                    ColorHex = color
                });
            }

            PrestamosRecientes.Clear();
            var recientes = db.Prestamos
                .Include(p => p.Cliente)
                .OrderByDescending(p => p.Id)
                .Take(6)
                .ToList();

            foreach (var p in recientes)
            {
                string colorEstado = p.Estado switch
                {
                    EstadoPrestamo.Activo => "#10B981",
                    EstadoPrestamo.Pagado => "#3B82F6",
                    EstadoPrestamo.EnMora => "#EF4444",
                    _ => "#475569"
                };

                PrestamosRecientes.Add(new PrestamoReciente
                {
                    Cliente = p.Cliente?.Nombre ?? "",
                    Monto = $"${p.Monto:N2}",
                    Fecha = p.FechaInicio.ToString("dd/MM/yyyy"),
                    Estado = p.Estado.ToString(),
                    ColorHex = colorEstado
                });
            }

            OnPropertyChanged(nameof(TotalClientes));
            OnPropertyChanged(nameof(TotalPrestamos));
            OnPropertyChanged(nameof(TotalMonto));
            OnPropertyChanged(nameof(TotalPagos));
            OnPropertyChanged(nameof(SparklineClientes));
            OnPropertyChanged(nameof(SparklinePrestamos));
            OnPropertyChanged(nameof(SparklineMonto));
            OnPropertyChanged(nameof(SparklinePagos));
            OnPropertyChanged(nameof(SeriesEstados));
            OnPropertyChanged(nameof(SeriesFrecuencia));
            OnPropertyChanged(nameof(EjesFrecuenciaX));
            OnPropertyChanged(nameof(EjesFrecuenciaY));
        }

        private static double[] ObtenerTendenciaClientes(AppDbContext db)
        {
            var hoy = DateTime.Today;
            var resultado = new double[6];
            for (int i = 5; i >= 0; i--)
            {
                var inicio = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-i);
                var fin = inicio.AddMonths(1);
                resultado[5 - i] = db.Clientes.Count(c => c.FechaRegistro < fin);
            }
            return resultado;
        }

        private static double[] ObtenerTendenciaPrestamos(AppDbContext db)
        {
            var hoy = DateTime.Today;
            var resultado = new double[6];
            for (int i = 5; i >= 0; i--)
            {
                var inicio = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-i);
                var fin = inicio.AddMonths(1);
                resultado[5 - i] = db.Prestamos.Count(p => p.FechaInicio < fin);
            }
            return resultado;
        }

        private static double[] ObtenerTendenciaMonto(AppDbContext db)
        {
            var hoy = DateTime.Today;
            var resultado = new double[6];
            for (int i = 5; i >= 0; i--)
            {
                var inicio = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-i);
                var fin = inicio.AddMonths(1);
                resultado[5 - i] = (double)db.Prestamos
                    .Where(p => p.FechaInicio < fin)
                    .AsEnumerable()
                    .Sum(p => p.Monto);
            }
            return resultado;
        }

        private static double[] ObtenerTendenciaPagos(AppDbContext db)
        {
            var hoy = DateTime.Today;
            var resultado = new double[6];
            for (int i = 5; i >= 0; i--)
            {
                var inicio = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-i);
                var fin = inicio.AddMonths(1);
                resultado[5 - i] = (double)db.Pagos
                    .Where(p => p.FechaPago >= inicio && p.FechaPago < fin)
                    .AsEnumerable()
                    .Sum(p => p.MontoPagado);
            }
            return resultado;
        }

        private static ISeries[] CrearSparkline(double[] valores, SKColor color)
        {
            return new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = valores,
                    Fill = null,
                    Stroke = new SolidColorPaint(color, 2),
                    GeometrySize = 0,
                    LineSmoothness = 0.8
                }
            };
        }
    }

    public class ProximoVencimiento
    {
        public string Cliente { get; set; } = "";
        public string Monto { get; set; } = "";
        public string Fecha { get; set; } = "";
        public string Estado { get; set; } = "";
        public string ColorHex { get; set; } = "";
    }

    public class PrestamoReciente
    {
        public string Cliente { get; set; } = "";
        public string Monto { get; set; } = "";
        public string Fecha { get; set; } = "";
        public string Estado { get; set; } = "";
        public string ColorHex { get; set; } = "";
    }
}
