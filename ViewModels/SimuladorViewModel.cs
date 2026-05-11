using System.Collections.ObjectModel;
using System.Windows.Input;
using AppPrestamos.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppPrestamos.ViewModels
{
    public partial class SimuladorViewModel : ObservableObject
    {
        [ObservableProperty]
        private decimal monto = 10000;

        [ObservableProperty]
        private decimal tasaInteres = 10;

        [ObservableProperty]
        private int tipoInteresSeleccionado;

        [ObservableProperty]
        private int frecuenciaSeleccionada;

        [ObservableProperty]
        private int numeroCuotas = 12;

        [ObservableProperty]
        private decimal montoTotal;

        [ObservableProperty]
        private decimal totalIntereses;

        [ObservableProperty]
        private decimal valorCuota;

        public bool HayResultado => CuotasSimuladas.Count > 0;

        public ObservableCollection<CuotaSimulada> CuotasSimuladas { get; } = [];
        public string[] TiposInteres => ["Simple", "Compuesto"];
        public string[] Frecuencias => ["Semanal", "Quincenal", "Mensual"];

        public ICommand CalcularCommand { get; }

        public SimuladorViewModel()
        {
            CalcularCommand = new RelayCommand(Calcular);
        }

        partial void OnMontoChanged(decimal value) => LimpiarResultado();
        partial void OnTasaInteresChanged(decimal value) => LimpiarResultado();
        partial void OnTipoInteresSeleccionadoChanged(int value) => LimpiarResultado();
        partial void OnFrecuenciaSeleccionadaChanged(int value) => LimpiarResultado();
        partial void OnNumeroCuotasChanged(int value) => LimpiarResultado();

        private void LimpiarResultado()
        {
            CuotasSimuladas.Clear();
            OnPropertyChanged(nameof(HayResultado));
        }

        private void Calcular()
        {
            CuotasSimuladas.Clear();

            if (Monto <= 0 || TasaInteres <= 0 || NumeroCuotas <= 0) return;

            var tipoInteres = TipoInteresSeleccionado == 0 ? TipoInteres.Simple : TipoInteres.Compuesto;
            var frecuencia = (FrecuenciaPago)(FrecuenciaSeleccionada + 1);

            var service = new Services.PrestamoService();
            MontoTotal = service.CalcularMontoTotal(Monto, TasaInteres, NumeroCuotas, tipoInteres);
            TotalIntereses = MontoTotal - Monto;
            ValorCuota = Math.Round(MontoTotal / NumeroCuotas, 2);

            decimal capitalCuota = Math.Round(Monto / NumeroCuotas, 2);
            decimal interesCuota = Math.Round(TotalIntereses / NumeroCuotas, 2);
            decimal saldo = MontoTotal;

            for (int i = 1; i <= NumeroCuotas; i++)
            {
                var fecha = service.CalcularFechaVencimiento(DateTime.Today, frecuencia, i);

                saldo -= ValorCuota;
                if (saldo < 0) saldo = 0;

                CuotasSimuladas.Add(new CuotaSimulada
                {
                    Numero = i,
                    Fecha = fecha.ToString("dd/MM/yyyy"),
                    Capital = i == NumeroCuotas
                        ? Monto - (capitalCuota * (NumeroCuotas - 1))
                        : capitalCuota,
                    Interes = i == NumeroCuotas
                        ? TotalIntereses - (interesCuota * (NumeroCuotas - 1))
                        : interesCuota,
                    Total = i == NumeroCuotas
                        ? MontoTotal - (ValorCuota * (NumeroCuotas - 1))
                        : ValorCuota,
                    Saldo = saldo
                });
            }

            OnPropertyChanged(nameof(HayResultado));
        }
    }

    public class CuotaSimulada
    {
        public int Numero { get; set; }
        public string Fecha { get; set; } = "";
        public decimal Capital { get; set; }
        public decimal Interes { get; set; }
        public decimal Total { get; set; }
        public decimal Saldo { get; set; }
    }
}
