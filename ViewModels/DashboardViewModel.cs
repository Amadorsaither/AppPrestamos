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
    /// <summary>ViewModel del Dashboard que carga y expone los datos del resumen principal</summary>
    public partial class DashboardViewModel : ObservableObject
    {
        private static readonly HashSet<string> NotificacionesVistas = new();
        /// <summary>Total de clientes registrados en el sistema</summary>
        [ObservableProperty]
        private string totalClientes = "0";

        /// <summary>Subtítulo descriptivo para la tarjeta de clientes</summary>
        [ObservableProperty]
        private string subtituloClientes = "Clientes Registrados";

        /// <summary>Total de préstamos activos actualmente</summary>
        [ObservableProperty]
        private string totalPrestamos = "0";

        /// <summary>Subtítulo descriptivo para la tarjeta de préstamos</summary>
        [ObservableProperty]
        private string subtituloPrestamos = "Pr\u00e9stamos Activos";

        /// <summary>Monto total prestado acumulado</summary>
        [ObservableProperty]
        private string totalMonto = "$0";

        /// <summary>Subtítulo descriptivo para la tarjeta de monto total</summary>
        [ObservableProperty]
        private string subtituloMonto = "Monto Total Prestado";

        /// <summary>Total de pagos realizados en el mes actual</summary>
        [ObservableProperty]
        private string totalPagos = "$0";

        /// <summary>Subtítulo descriptivo para la tarjeta de pagos</summary>
        [ObservableProperty]
        private string subtituloPagos = "Pagos del Mes";

        /// <summary>Cantidad de préstamos en estado Activo</summary>
        [ObservableProperty]
        private int activosCount;

        /// <summary>Cantidad de préstamos en estado Pagado</summary>
        [ObservableProperty]
        private int pagadosCount;

        /// <summary>Cantidad de préstamos en estado En Mora</summary>
        [ObservableProperty]
        private int moraCount;

        /// <summary>Número de notificaciones activas</summary>
        [ObservableProperty]
        private int notificacionesCount;

        /// <summary>Indica si el panel de notificaciones está abierto</summary>
        public bool TieneNotificaciones => NotificacionesCount > 0;

        partial void OnNotificacionesCountChanged(int value)
        {
            OnPropertyChanged(nameof(TieneNotificaciones));
        }

        /// <summary>Abre o cierra el panel de notificaciones</summary>
        [RelayCommand]
        private void NotificacionClick(NotificacionItem item)
        {
            if (!string.IsNullOrEmpty(item.ItemKey))
                NotificacionesVistas.Add(item.ItemKey);

            Notificaciones.Remove(item);
            NotificacionesCount = Notificaciones.Count;

            if (!string.IsNullOrEmpty(item.Seccion))
                WeakReferenceMessenger.Default.Send(new NavigationMessage(item.Seccion));
        }

        /// <summary>Series de datos para el gráfico circular de estados de préstamos</summary>
        public ISeries[] SeriesEstados { get; private set; } = [];

        /// <summary>Colección de próximos vencimientos de cuotas</summary>
        public ObservableCollection<ProximoVencimiento> Vencimientos { get; } = [];
        /// <summary>Colección de préstamos registrados recientemente</summary>
        public ObservableCollection<PrestamoReciente> PrestamosRecientes { get; } = [];
        /// <summary>Colección de notificaciones activas</summary>
        public ObservableCollection<NotificacionItem> Notificaciones { get; } = [];

        /// <summary>Navega a la sección de Clientes para registrar uno nuevo</summary>
        [RelayCommand]
        private void NuevoCliente() =>
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Clientes"));

        /// <summary>Navega a la sección de Préstamos para registrar uno nuevo</summary>
        [RelayCommand]
        private void NuevoPrestamo() =>
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Prestamos"));

        /// <summary>Navega a la sección de Pagos para registrar un pago</summary>
        [RelayCommand]
        private void RegistrarPago() =>
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Pagos"));

        /// <summary>Navega a la sección de Reportes de mora</summary>
        [RelayCommand]
        private void ReporteMora() =>
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Reportes"));

        /// <summary>Navega directamente al pago de una cuota específica</summary>
        [RelayCommand]
        private void IrAPago(ProximoVencimiento vencimiento)
        {
            WeakReferenceMessenger.Default.Send(new NavigateToPagoConCuotaMessage(vencimiento.CuotaId));
        }

        public DashboardViewModel()
        {
            CargarDatos();
        }

        /// <summary>Carga todos los datos del Dashboard desde la base de datos</summary>
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

            var blanco = SKColors.White;
            SeriesEstados = new ISeries[]
            {
                new PieSeries<double> { Values = [activos], Name = "Activos", Fill = new SolidColorPaint(verde),
                    Stroke = new SolidColorPaint(blanco, 3), HoverPushout = 8, InnerRadius = 65 },
                new PieSeries<double> { Values = [pagados], Name = "Pagados", Fill = new SolidColorPaint(azul),
                    Stroke = new SolidColorPaint(blanco, 3), HoverPushout = 8, InnerRadius = 65 },
                new PieSeries<double> { Values = [mora], Name = "En Mora", Fill = new SolidColorPaint(rojo),
                    Stroke = new SolidColorPaint(blanco, 3), HoverPushout = 8, InnerRadius = 65 }
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
                var key = $"prestamo-{p.Id}";
                if (NotificacionesVistas.Contains(key)) continue;
                Notificaciones.Add(new NotificacionItem
                {
                    ItemKey = key,
                    Titulo = $"Nuevo préstamo - {p.Cliente?.Nombre ?? ""}",
                    Mensaje = $"${p.Monto:N2} — {p.NumeroCuotas} cuotas",
                    Tipo = "info",
                    Seccion = "Prestamos"
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
                var key = $"cuota-{c.Id}";
                if (NotificacionesVistas.Contains(key)) continue;
                var diasVencida = (hoy - c.FechaVencimiento).Days;
                Notificaciones.Add(new NotificacionItem
                {
                    ItemKey = key,
                    Titulo = $"Cuota vencida - {c.Prestamo?.Cliente?.Nombre ?? ""}",
                    Mensaje = $"Cuota #{c.NumeroCuota} — ${c.SaldoPendiente:N2} — {diasVencida} días de atraso",
                    Tipo = "alerta",
                    Seccion = "Cuotas"
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
                var key = $"cuota-{c.Id}";
                if (NotificacionesVistas.Contains(key)) continue;
                Notificaciones.Add(new NotificacionItem
                {
                    ItemKey = key,
                    Titulo = $"Próximo vencimiento - {c.Prestamo?.Cliente?.Nombre ?? ""}",
                    Mensaje = $"Cuota #{c.NumeroCuota} — ${c.SaldoPendiente:N2} — Vence {c.FechaVencimiento:dd/MM/yyyy}",
                    Tipo = "info",
                    Seccion = "Cuotas"
                });
            }

            var pagosHoy = db.Pagos.Include(p => p.Cuota.Prestamo.Cliente)
                .Where(p => p.FechaPago.Date == hoy)
                .OrderByDescending(p => p.Id)
                .Take(5)
                .ToList();

            foreach (var p in pagosHoy)
            {
                var key = $"pago-{p.Id}";
                if (NotificacionesVistas.Contains(key)) continue;
                Notificaciones.Add(new NotificacionItem
                {
                    ItemKey = key,
                    Titulo = $"Pago registrado - {p.Cuota?.Prestamo?.Cliente?.Nombre ?? ""}",
                    Mensaje = $"${p.MontoPagado:N2} — Cuota #{p.Cuota?.NumeroCuota}",
                    Tipo = "info",
                    Seccion = "Pagos"
                });
            }

            var nuevosClientes = db.Clientes
                .Where(c => c.FechaRegistro.Date == hoy)
                .OrderByDescending(c => c.Id)
                .Take(3)
                .ToList();

            foreach (var c in nuevosClientes)
            {
                var key = $"cliente-{c.Id}";
                if (NotificacionesVistas.Contains(key)) continue;
                Notificaciones.Add(new NotificacionItem
                {
                    ItemKey = key,
                    Titulo = $"Nuevo cliente - {c.Nombre}",
                    Mensaje = $"Cédula: {c.Cedula}",
                    Tipo = "info",
                    Seccion = "Clientes"
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
        public string ItemKey { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public string Tipo { get; set; } = "info";
        public string Seccion { get; set; } = "";
        public string Color => Tipo == "alerta" ? "#EF4444" : "#3B82F6";
        public Brush ColorBrush => new SolidColorBrush((Color)ColorConverter.ConvertFromString(Color));
    }
}
