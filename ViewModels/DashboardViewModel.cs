using System.Collections.ObjectModel;
using System.Windows.Media;
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
        private string totalPrestamos = "0";

        [ObservableProperty]
        private string subtituloPrestamos = "Pr\u00e9stamos Activos";

        [ObservableProperty]
        private string totalMonto = "$0";

        [ObservableProperty]
        private string subtituloMonto = "Monto Total Prestado";

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
        private int notificacionesCount;

        [ObservableProperty]
        private bool isNotificacionesOpen;

        public bool TieneNotificaciones => NotificacionesCount > 0;

        partial void OnNotificacionesCountChanged(int value)
        {
            OnPropertyChanged(nameof(TieneNotificaciones));
        }

        [RelayCommand]
        private void ToggleNotificaciones()
        {
            IsNotificacionesOpen = !IsNotificacionesOpen;
        }

        public ISeries[] SeriesEstados { get; private set; } = [];

        public ObservableCollection<ProximoVencimiento> Vencimientos { get; } = [];
        public ObservableCollection<PrestamoReciente> PrestamosRecientes { get; } = [];
        public ObservableCollection<NotificacionItem> Notificaciones { get; } = [];

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

        [RelayCommand]
        private void IrAPago(ProximoVencimiento vencimiento)
        {
            WeakReferenceMessenger.Default.Send(new NavigateToPagoConCuotaMessage(vencimiento.CuotaId));
        }

        public DashboardViewModel()
        {
            CargarDatos();
        }

        public void CargarDatos()
        {
            using var db = new AppDbContext();

            var totalClientesDb = db.Clientes.Count();
            TotalClientes = totalClientesDb.ToString("N0");
            SubtituloClientes = "Clientes Registrados";

            var prestamosActivos = db.Prestamos.Count(p => p.Estado == EstadoPrestamo.Activo);
            TotalPrestamos = prestamosActivos.ToString("N0");
            SubtituloPrestamos = $"{prestamosActivos} en curso";

            var montoTotal = db.Prestamos.AsEnumerable().Sum(p => p.Monto);
            TotalMonto = $"${montoTotal:N2}";
            SubtituloMonto = $"${montoTotal:N0} desembolsados";

            var hoy = DateTime.Today;
            var pagosMes = db.Pagos
                .Where(p => p.FechaPago.Year == hoy.Year && p.FechaPago.Month == hoy.Month)
                .AsEnumerable()
                .Sum(p => p.MontoPagado);
            TotalPagos = $"${pagosMes:N2}";
            SubtituloPagos = $"{pagosMes:N0} recaudados";

            var activos = db.Prestamos.Count(p => p.Estado == EstadoPrestamo.Activo);
            var pagados = db.Prestamos.Count(p => p.Estado == EstadoPrestamo.Pagado);
            var mora = db.Prestamos.Count(p => p.Estado == EstadoPrestamo.EnMora);

            ActivosCount = activos;
            PagadosCount = pagados;
            MoraCount = mora;

            var verde = SKColor.Parse("#10B981");
            var azul = SKColor.Parse("#3B82F6");
            var rojo = SKColor.Parse("#EF4444");

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
                    CuotaId = c.Id,
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

            Notificaciones.Clear();

            var nuevosPrestamos = db.Prestamos.Include(p => p.Cliente)
                .Where(p => p.FechaInicio.Date == hoy)
                .OrderByDescending(p => p.Id)
                .Take(3)
                .ToList();

            foreach (var p in nuevosPrestamos)
            {
                Notificaciones.Add(new NotificacionItem
                {
                    Titulo = $"Nuevo préstamo - {p.Cliente?.Nombre ?? ""}",
                    Mensaje = $"${p.Monto:N2} — {p.NumeroCuotas} cuotas",
                    Tipo = "info"
                });
            }

            var vencidasHoy = db.Cuotas
                .Include(c => c.Prestamo).ThenInclude(p => p.Cliente)
                .Where(c => c.Estado == EstadoCuota.Vencida || c.Estado == EstadoCuota.Parcial || (c.Estado == EstadoCuota.Pendiente && c.FechaVencimiento <= hoy))
                .OrderBy(c => c.FechaVencimiento)
                .Take(5)
                .ToList();

            foreach (var c in vencidasHoy)
            {
                var diasVencida = (hoy - c.FechaVencimiento).Days;
                Notificaciones.Add(new NotificacionItem
                {
                    Titulo = $"Cuota vencida - {c.Prestamo?.Cliente?.Nombre ?? ""}",
                    Mensaje = $"Cuota #{c.NumeroCuota} — ${c.SaldoPendiente:N2} — {diasVencida} días de atraso",
                    Tipo = "alerta"
                });
            }

            var proximas3 = db.Cuotas
                .Include(c => c.Prestamo).ThenInclude(p => p.Cliente)
                .Where(c => (c.Estado == EstadoCuota.Pendiente || c.Estado == EstadoCuota.Parcial) && c.FechaVencimiento > hoy && c.FechaVencimiento <= hoy.AddDays(3))
                .OrderBy(c => c.FechaVencimiento)
                .Take(3)
                .ToList();

            foreach (var c in proximas3)
            {
                Notificaciones.Add(new NotificacionItem
                {
                    Titulo = $"Próximo vencimiento - {c.Prestamo?.Cliente?.Nombre ?? ""}",
                    Mensaje = $"Cuota #{c.NumeroCuota} — ${c.SaldoPendiente:N2} — Vence {c.FechaVencimiento:dd/MM/yyyy}",
                    Tipo = "info"
                });
            }

            NotificacionesCount = Notificaciones.Count;

            OnPropertyChanged(nameof(SeriesEstados));
            OnPropertyChanged(nameof(Notificaciones));
        }
    }

    public class ProximoVencimiento
    {
        public int CuotaId { get; set; }
        public string Cliente { get; set; } = "";
        public string Monto { get; set; } = "";
        public string Fecha { get; set; } = "";
        public string Estado { get; set; } = "";
        public string ColorHex { get; set; } = "";
        public Brush ColorBrush => new SolidColorBrush((Color)ColorConverter.ConvertFromString(ColorHex));
        public Brush ColorBrushLight
        {
            get
            {
                var c = (Color)ColorConverter.ConvertFromString(ColorHex);
                return new SolidColorBrush(Color.FromArgb(38, c.R, c.G, c.B));
            }
        }
    }

    public class PrestamoReciente
    {
        public string Cliente { get; set; } = "";
        public string Monto { get; set; } = "";
        public string Fecha { get; set; } = "";
        public string Estado { get; set; } = "";
        public string ColorHex { get; set; } = "";
        public Brush ColorBrush => new SolidColorBrush((Color)ColorConverter.ConvertFromString(ColorHex));
        public Brush ColorBrushLight
        {
            get
            {
                var c = (Color)ColorConverter.ConvertFromString(ColorHex);
                return new SolidColorBrush(Color.FromArgb(38, c.R, c.G, c.B));
            }
        }
    }

    public class NotificacionItem
    {
        public string Titulo { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public string Tipo { get; set; } = "info";
        public string Color => Tipo == "alerta" ? "#EF4444" : "#3B82F6";
        public Brush ColorBrush => new SolidColorBrush((Color)ColorConverter.ConvertFromString(Color));
    }
}
