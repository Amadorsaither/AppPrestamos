using System.Collections.ObjectModel;
using System.Windows.Input;
using AppPrestamos.Data;
using AppPrestamos.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace AppPrestamos.ViewModels
{
    public partial class CalendarioViewModel : ObservableObject
    {
        [ObservableProperty]
        private int anio;

        [ObservableProperty]
        private int mes;

        [ObservableProperty]
        private string tituloMes = "";

        public ObservableCollection<DiaCalendario> Dias { get; } = [];
        public ObservableCollection<PagoCalendario> PagosDelDia { get; } = [];

        public ICommand MesAnteriorCommand { get; }
        public ICommand MesSiguienteCommand { get; }
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

    public class DiaCalendario
    {
        public int Numero { get; set; }
        public bool EsVacio { get; set; }
        public bool EsHoy { get; set; }
        public bool TienePago { get; set; }
        public int TotalPagos { get; set; }
        public int Vencidos { get; set; }
        public int Pendientes { get; set; }
        public DateTime Fecha { get; set; }
        public string Tooltip => TienePago ? $"{TotalPagos} pago(s)" : "";
        public string ColorFondo => EsHoy ? "#EFF6FF" : "Transparent";
        public string ColorNumero => EsHoy ? "#3B82F6" : "#0F172A";
        public string TextoNumero => EsVacio ? "" : Numero.ToString();
    }

    public class PagoCalendario
    {
        public string Cliente { get; set; } = "";
        public decimal Monto { get; set; }
        public string Estado { get; set; } = "";
        public string ColorEstado { get; set; } = "";
    }
}
