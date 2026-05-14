using System.Collections.ObjectModel;
using System.Windows.Input;
using AppPrestamos.Data;
using AppPrestamos.Enums;
using AppPrestamos.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using SkiaSharp;

namespace AppPrestamos.ViewModels
{
    /// <summary>ViewModel para la sección de reportes, muestra estadísticas y gráficos del sistema</summary>
    public partial class ReportesViewModel : ObservableObject
    {
        /// <summary>Total de clientes registrados en el sistema</summary>
        [ObservableProperty]
        private string totalClientes = "0";

        /// <summary>Total de préstamos registrados en el sistema</summary>
        [ObservableProperty]
        private string totalPrestamos = "0";

        /// <summary>Total de dinero desembolsado en préstamos</summary>
        [ObservableProperty]
        private string montoDesembolsado = "$0";

        /// <summary>Total de dinero cobrado a través de pagos</summary>
        [ObservableProperty]
        private string montoCobrado = "$0";

        /// <summary>Saldo total pendiente por cobrar</summary>
        [ObservableProperty]
        private string saldoPendiente = "$0";

        /// <summary>Cantidad de cuotas en estado vencido (mora)</summary>
        [ObservableProperty]
        private string clientesEnMora = "0";

        /// <summary>Series del gráfico circular con el estado de los préstamos (activos, pagados, en mora)</summary>
        public ISeries[] SeriesEstadoPrestamos { get; private set; } = [];
        /// <summary>Series del gráfico de barras con ingresos mensuales</summary>
        public ISeries[] SeriesIngresosMensuales { get; private set; } = [];
        /// <summary>Eje X del gráfico de ingresos mensuales (etiquetas de meses)</summary>
        public Axis[] EjesIngresosX { get; private set; } = [];
        /// <summary>Eje Y del gráfico de ingresos mensuales (valores monetarios)</summary>
        public Axis[] EjesIngresosY { get; private set; } = [];

        /// <summary>Lista de clientes con cuotas vencidas para la tabla de mora</summary>
        public ObservableCollection<ClienteMora> ClientesMora { get; } = [];
        /// <summary>Resumen mensual de préstamos y cobros del año actual</summary>
        public ObservableCollection<ResumenAnual> ResumenAnual { get; } = [];

        /// <summary>Comando para exportar el reporte a PDF</summary>
        public ICommand ExportarPdfCommand { get; }
        /// <summary>Comando para exportar el reporte a Excel</summary>
        public ICommand ExportarExcelCommand { get; }

        public ReportesViewModel()
        {
            CargarReportes();

            ExportarPdfCommand = new RelayCommand(ExportarPdf);
            ExportarExcelCommand = new RelayCommand(ExportarExcel);
        }

        private void ExportarPdf()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"Reporte_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                var service = new ReportExportService();
                service.ExportarPdf(dialog.FileName, this);
            }
        }

        private void ExportarExcel()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Reporte_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                var service = new ReportExportService();
                service.ExportarExcel(dialog.FileName, this);
            }
        }

        public void CargarReportes()
        {
            using var db = new AppDbContext();

            TotalClientes = db.Clientes.Count().ToString("N0");
            TotalPrestamos = db.Prestamos.Count().ToString("N0");

            var desembolsado = db.Prestamos.AsEnumerable().Sum(p => p.Monto);
            MontoDesembolsado = $"${desembolsado:N2}";

            var cobrado = db.Pagos.AsEnumerable().Sum(p => p.MontoPagado);
            MontoCobrado = $"${cobrado:N2}";

            var pendiente = db.Cuotas.AsEnumerable().Sum(c => c.SaldoPendiente);
            SaldoPendiente = $"${pendiente:N2}";

            var enMora = db.Cuotas.Count(c => c.Estado == EstadoCuota.Vencida);
            ClientesEnMora = enMora.ToString("N0");

            var azul = SKColor.Parse("#3B82F6");
            var verde = SKColor.Parse("#10B981");
            var naranja = SKColor.Parse("#F59E0B");
            var morado = SKColor.Parse("#8B5CF6");
            var rojo = SKColor.Parse("#EF4444");

            var activos = db.Prestamos.Count(p => p.Estado == EstadoPrestamo.Activo);
            var pagados = db.Prestamos.Count(p => p.Estado == EstadoPrestamo.Pagado);
            var mora = db.Prestamos.Count(p => p.Estado == EstadoPrestamo.EnMora);

            var blanco = SKColors.White;
            SeriesEstadoPrestamos = new ISeries[]
            {
                new PieSeries<double> { Values = [activos], Name = "Activos", Fill = new SolidColorPaint(verde),
                    Stroke = new SolidColorPaint(blanco, 3), HoverPushout = 8, InnerRadius = 55 },
                new PieSeries<double> { Values = [pagados], Name = "Pagados", Fill = new SolidColorPaint(azul),
                    Stroke = new SolidColorPaint(blanco, 3), HoverPushout = 8, InnerRadius = 55 },
                new PieSeries<double> { Values = [mora], Name = "En Mora", Fill = new SolidColorPaint(rojo),
                    Stroke = new SolidColorPaint(blanco, 3), HoverPushout = 8, InnerRadius = 55 }
            };

            var ingresosMensuales = new double[12];
            var labels = new string[12];
            var hoy = DateTime.Today;

            for (int i = 11; i >= 0; i--)
            {
                var mes = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-i);
                labels[11 - i] = mes.ToString("MMM");
                var inicio = mes;
                var fin = mes.AddMonths(1);

                ingresosMensuales[11 - i] = (double)db.Pagos
                    .Where(p => p.FechaPago >= inicio && p.FechaPago < fin)
                    .AsEnumerable()
                    .Sum(p => p.MontoPagado);
            }

            SeriesIngresosMensuales = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = ingresosMensuales,
                    Fill = new SolidColorPaint(azul),
                    Stroke = null,
                    MaxBarWidth = 30
                }
            };

            EjesIngresosX = new Axis[]
            {
                new() { Labels = labels, LabelsRotation = 0,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#334155")), TextSize = 11 }
            };

            EjesIngresosY = new Axis[]
            {
                new() { Labeler = v => $"${v:N0}",
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#334155")), TextSize = 11,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E2E8F0"), 1) }
            };

            ClientesMora.Clear();
            var cuotasVencidas = db.Cuotas
                .Include(c => c.Prestamo).ThenInclude(p => p.Cliente)
                .Where(c => c.Estado == EstadoCuota.Vencida)
                .OrderBy(c => c.FechaVencimiento)
                .Take(20)
                .ToList();

            foreach (var c in cuotasVencidas)
            {
                var diasVencidos = (hoy - c.FechaVencimiento).Days;
                ClientesMora.Add(new ClienteMora
                {
                    Cliente = c.Prestamo?.Cliente?.Nombre ?? "",
                    PrestamoId = c.PrestamoId,
                    CuotaNumero = c.NumeroCuota,
                    Monto = c.SaldoPendiente,
                    Vencimiento = c.FechaVencimiento.ToString("dd/MM/yyyy"),
                    DiasVencido = diasVencidos,
                    ColorDias = diasVencidos > 30 ? "#EF4444" : diasVencidos > 15 ? "#F59E0B" : "#3B82F6"
                });
            }

            ResumenAnual.Clear();
            for (int i = 0; i < 12; i++)
            {
                var mes = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-11 + i);
                var inicio = mes;
                var fin = mes.AddMonths(1);

                var prestamosMes = db.Prestamos.Count(p => p.FechaInicio >= inicio && p.FechaInicio < fin);
                var pagosMes = db.Pagos
                    .Where(p => p.FechaPago >= inicio && p.FechaPago < fin)
                    .AsEnumerable()
                    .Sum(p => p.MontoPagado);

                ResumenAnual.Add(new ResumenAnual
                {
                    Mes = mes.ToString("MMMM yyyy"),
                    Prestamos = prestamosMes,
                    Cobrado = pagosMes
                });
            }

            OnPropertyChanged(nameof(SeriesEstadoPrestamos));
            OnPropertyChanged(nameof(SeriesIngresosMensuales));
            OnPropertyChanged(nameof(EjesIngresosX));
            OnPropertyChanged(nameof(EjesIngresosY));
        }
    }

    /// <summary>Modelo que representa un cliente con cuota vencida para la tabla de mora</summary>
    public class ClienteMora
    {
        /// <summary>Nombre del cliente en mora</summary>
        public string Cliente { get; set; } = "";
        /// <summary>Identificador del préstamo asociado</summary>
        public int PrestamoId { get; set; }
        /// <summary>Número de cuota vencida</summary>
        public int CuotaNumero { get; set; }
        /// <summary>Monto pendiente de la cuota vencida</summary>
        public decimal Monto { get; set; }
        /// <summary>Fecha de vencimiento de la cuota</summary>
        public string Vencimiento { get; set; } = "";
        /// <summary>Cantidad de días que lleva vencida la cuota</summary>
        public int DiasVencido { get; set; }
        /// <summary>Color hexadecimal que indica la gravedad de la mora (rojo, naranja, azul)</summary>
        public string ColorDias { get; set; } = "";
    }

    /// <summary>Modelo que representa el resumen mensual de préstamos y cobros</summary>
    public class ResumenAnual
    {
        /// <summary>Nombre del mes (ej. enero 2026)</summary>
        public string Mes { get; set; } = "";
        /// <summary>Cantidad de préstamos otorgados en el mes</summary>
        public int Prestamos { get; set; }
        /// <summary>Total cobrado en el mes</summary>
        public decimal Cobrado { get; set; }
    }
}
