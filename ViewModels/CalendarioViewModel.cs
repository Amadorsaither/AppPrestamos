using System.Collections.ObjectModel;
using System.Windows.Input;
using AppPrestamos.Data;
using AppPrestamos.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace AppPrestamos.ViewModels
{
    /// <summary>ViewModel para el calendario de pagos, muestra las cuotas agrupadas por fecha de vencimiento</summary>
    public partial class CalendarioViewModel : ObservableObject
    {
        /// <summary>Año actual del calendario visible</summary>
        [ObservableProperty]
        private int anio;

        /// <summary>Mes actual del calendario visible (1-12)</summary>
        [ObservableProperty]
        private int mes;

        /// <summary>Título del mes y año mostrado en el calendario</summary>
        [ObservableProperty]
        private string tituloMes = "";

        /// <summary>Días del calendario del mes actual con información de pagos</summary>
        public ObservableCollection<DiaCalendario> Dias { get; } = [];
        /// <summary>Cuotas con vencimiento en el día seleccionado del calendario</summary>
        public ObservableCollection<PagoCalendario> PagosDelDia { get; } = [];

        /// <summary>Comando para navegar al mes anterior en el calendario</summary>
        public ICommand MesAnteriorCommand { get; }
        /// <summary>Comando para navegar al mes siguiente en el calendario</summary>
        public ICommand MesSiguienteCommand { get; }
        /// <summary>Comando para seleccionar un día específico y ver sus pagos</summary>
        public ICommand SeleccionarDiaCommand { get; }

        private DateTime diaSeleccionado;

        public CalendarioViewModel()
        {
            var hoy = DateTime.Today;
            Anio = hoy.Year;
            Mes = hoy.Month;

            MesAnteriorCommand = new RelayCommand(() => CambiarMes(-1));
            MesSiguienteCommand = new RelayCommand(() => CambiarMes(1));
            SeleccionarDiaCommand = new RelayCommand<string>(SeleccionarDia);

            CargarCalendario();
        }

        partial void OnAnioChanged(int value) => CargarCalendario();
        partial void OnMesChanged(int value) => CargarCalendario();

        private void CambiarMes(int delta)
        {
            var fecha = new DateTime(Anio, Mes, 1).AddMonths(delta);
            Anio = fecha.Year;
            Mes = fecha.Month;
        }

        private void CargarCalendario()
        {
            TituloMes = new DateTime(Anio, Mes, 1).ToString("MMMM yyyy");
            Dias.Clear();

            var primerDia = new DateTime(Anio, Mes, 1);
            var ultimoDia = primerDia.AddMonths(1).AddDays(-1);
            int diaSemanaInicio = (int)primerDia.DayOfWeek;

            using var db = new AppDbContext();
            var inicioMes = new DateTime(Anio, Mes, 1);
            var finMes = inicioMes.AddMonths(1);

            var cuotasMes = db.Cuotas
                .Include(c => c.Prestamo).ThenInclude(p => p.Cliente)
                .Where(c => c.FechaVencimiento >= inicioMes && c.FechaVencimiento < finMes)
                .OrderBy(c => c.FechaVencimiento)
                .ToList();

            var cuotasPorDia = cuotasMes.GroupBy(c => c.FechaVencimiento.Day)
                .ToDictionary(g => g.Key, g => g.ToList());

            for (int i = 0; i < diaSemanaInicio; i++)
                Dias.Add(new DiaCalendario { EsVacio = true });

            for (int dia = 1; dia <= ultimoDia.Day; dia++)
            {
                var fecha = new DateTime(Anio, Mes, dia);
                bool tienePago = cuotasPorDia.ContainsKey(dia);
                int totalPagos = tienePago ? cuotasPorDia[dia].Count : 0;
                int vencidos = tienePago
                    ? cuotasPorDia[dia].Count(c => c.Estado == EstadoCuota.Vencida)
                    : 0;
                int pendientes = tienePago
                    ? cuotasPorDia[dia].Count(c => c.Estado == EstadoCuota.Pendiente)
                    : 0;

                Dias.Add(new DiaCalendario
                {
                    Numero = dia,
                    EsHoy = fecha.Date == DateTime.Today,
                    TienePago = tienePago,
                    TotalPagos = totalPagos,
                    Vencidos = vencidos,
                    Pendientes = pendientes,
                    Fecha = fecha
                });
            }

            diaSeleccionado = DateTime.Today;
            CargarPagosDia(diaSeleccionado);
        }

        private void SeleccionarDia(string? diaStr)
        {
            if (string.IsNullOrEmpty(diaStr) || !int.TryParse(diaStr, out int dia)) return;
            diaSeleccionado = new DateTime(Anio, Mes, dia);
            CargarPagosDia(diaSeleccionado);
        }

        private void CargarPagosDia(DateTime fecha)
        {
            PagosDelDia.Clear();

            using var db = new AppDbContext();
            var cuotas = db.Cuotas
                .Include(c => c.Prestamo).ThenInclude(p => p.Cliente)
                .Where(c => c.FechaVencimiento.Year == fecha.Year
                    && c.FechaVencimiento.Month == fecha.Month
                    && c.FechaVencimiento.Day == fecha.Day)
                .ToList();

            foreach (var c in cuotas)
            {
                PagosDelDia.Add(new PagoCalendario
                {
                    Cliente = c.Prestamo?.Cliente?.Nombre ?? "",
                    Monto = c.SaldoPendiente,
                    Estado = c.Estado switch
                    {
                        EstadoCuota.Pendiente => "Pendiente",
                        EstadoCuota.Pagada => "Pagada",
                        EstadoCuota.Vencida => "Vencida",
                        EstadoCuota.Parcial => "Parcial",
                        _ => ""
                    },
                    ColorEstado = c.Estado switch
                    {
                        EstadoCuota.Pendiente => "#3B82F6",
                        EstadoCuota.Pagada => "#10B981",
                        EstadoCuota.Vencida => "#EF4444",
                        EstadoCuota.Parcial => "#F59E0B",
                        _ => "#94A3B8"
                    }
                });
            }
        }
    }

    /// <summary>Modelo que representa un día en la cuadrícula del calendario con información de pagos</summary>
    public class DiaCalendario
    {
        /// <summary>Número del día (1-31)</summary>
        public int Numero { get; set; }
        /// <summary>Indica si es un día vacío (fuera del mes) en la cuadrícula</summary>
        public bool EsVacio { get; set; }
        /// <summary>Indica si este día es la fecha actual</summary>
        public bool EsHoy { get; set; }
        /// <summary>Indica si hay pagos programados para este día</summary>
        public bool TienePago { get; set; }
        /// <summary>Cantidad total de pagos en este día</summary>
        public int TotalPagos { get; set; }
        /// <summary>Cantidad de cuotas vencidas en este día</summary>
        public int Vencidos { get; set; }
        /// <summary>Cantidad de cuotas pendientes en este día</summary>
        public int Pendientes { get; set; }
        /// <summary>Fecha completa del día</summary>
        public DateTime Fecha { get; set; }
        /// <summary>Texto de tooltip informativo para el día</summary>
        public string Tooltip => TienePago ? $"{TotalPagos} pago(s)" : "";
        /// <summary>Color de fondo del día (resalta el día actual)</summary>
        public string ColorFondo => EsHoy ? "#EFF6FF" : "Transparent";
        /// <summary>Color del número del día (resalta el día actual)</summary>
        public string ColorNumero => EsHoy ? "#3B82F6" : "#0F172A";
        /// <summary>Texto del número del día (vacío para celdas sin día)</summary>
        public string TextoNumero => EsVacio ? "" : Numero.ToString();
    }

    /// <summary>Modelo que representa una cuota con vencimiento en una fecha específica del calendario</summary>
    public class PagoCalendario
    {
        /// <summary>Nombre del cliente asociado a la cuota</summary>
        public string Cliente { get; set; } = "";
        /// <summary>Monto pendiente de la cuota</summary>
        public decimal Monto { get; set; }
        /// <summary>Estado de la cuota (Pendiente, Pagada, Vencida, Parcial)</summary>
        public string Estado { get; set; } = "";
        /// <summary>Color hexadecimal que representa visualmente el estado</summary>
        public string ColorEstado { get; set; } = "";
    }
}
