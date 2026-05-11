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
    public partial class ReportesViewModel : ObservableObject
    {
        [ObservableProperty]
        private string totalClientes = "0";

        [ObservableProperty]
        private string totalPrestamos = "0";

        [ObservableProperty]
        private string montoDesembolsado = "$0";

        [ObservableProperty]
        private string montoCobrado = "$0";

        [ObservableProperty]
        private string saldoPendiente = "$0";

        [ObservableProperty]
        private string clientesEnMora = "0";

        public ISeries[] SeriesEstadoPrestamos { get; private set; } = [];
        public ISeries[] SeriesIngresosMensuales { get; private set; } = [];
        public Axis[] EjesIngresosX { get; private set; } = [];
        public Axis[] EjesIngresosY { get; private set; } = [];

        public ObservableCollection<ClienteMora> ClientesMora { get; } = [];
        public ObservableCollection<ResumenAnual> ResumenAnual { get; } = [];

        public ICommand ExportarPdfCommand { get; }
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

            SeriesEstadoPrestamos = new ISeries[]
            {
                new PieSeries<double> { Values = [activos], Name = "Activos", Fill = new SolidColorPaint(verde),
                    HoverPushout = 5, InnerRadius = 40,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#1E293B")), DataLabelsSize = 12 },
                new PieSeries<double> { Values = [pagados], Name = "Pagados", Fill = new SolidColorPaint(azul),
                    InnerRadius = 40,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#1E293B")), DataLabelsSize = 12 },
                new PieSeries<double> { Values = [mora], Name = "En Mora", Fill = new SolidColorPaint(rojo),
                    InnerRadius = 40,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#1E293B")), DataLabelsSize = 12 }
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

    public class ClienteMora
    {
        public string Cliente { get; set; } = "";
        public int PrestamoId { get; set; }
        public int CuotaNumero { get; set; }
        public decimal Monto { get; set; }
        public string Vencimiento { get; set; } = "";
        public int DiasVencido { get; set; }
        public string ColorDias { get; set; } = "";
    }

    public class ResumenAnual
    {
        public string Mes { get; set; } = "";
        public int Prestamos { get; set; }
        public decimal Cobrado { get; set; }
    }
}
