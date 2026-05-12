using System.Collections.ObjectModel;
using System.Windows.Input;
using AppPrestamos.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppPrestamos.ViewModels
{
    /// <summary>ViewModel para el simulador de préstamos, calcula cuotas según monto, tasa e interés</summary>
    public partial class SimuladorViewModel : ObservableObject
    {
        /// <summary>Monto del préstamo a simular</summary>
        [ObservableProperty]
        private decimal monto = 10000;

        /// <summary>Tasa de interés aplicada al préstamo (porcentaje)</summary>
        [ObservableProperty]
        private decimal tasaInteres = 10;

        /// <summary>Tipo de interés seleccionado (0 = Simple, 1 = Compuesto)</summary>
        [ObservableProperty]
        private int tipoInteresSeleccionado;

        /// <summary>Frecuencia de pago seleccionada (0 = Semanal, 1 = Quincenal, 2 = Mensual)</summary>
        [ObservableProperty]
        private int frecuenciaSeleccionada;

        /// <summary>Número de cuotas del préstamo a simular</summary>
        [ObservableProperty]
        private int numeroCuotas = 12;

        /// <summary>Monto total del préstamo incluyendo intereses calculados</summary>
        [ObservableProperty]
        private decimal montoTotal;

        /// <summary>Total de intereses generados por el préstamo</summary>
        [ObservableProperty]
        private decimal totalIntereses;

        /// <summary>Valor de cada cuota calculado (monto total / número de cuotas)</summary>
        [ObservableProperty]
        private decimal valorCuota;

        /// <summary>Indica si hay resultados de simulación disponibles</summary>
        public bool HayResultado => CuotasSimuladas.Count > 0;

        /// <summary>Lista de cuotas simuladas con su desglose de capital, interés y saldo</summary>
        public ObservableCollection<CuotaSimulada> CuotasSimuladas { get; } = [];
        /// <summary>Opciones de tipo de interés disponibles para la simulación</summary>
        public string[] TiposInteres => ["Simple", "Compuesto"];
        /// <summary>Opciones de frecuencia de pago disponibles para la simulación</summary>
        public string[] Frecuencias => ["Semanal", "Quincenal", "Mensual"];

        /// <summary>Comando para ejecutar la simulación y calcular las cuotas</summary>
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

    /// <summary>Modelo que representa una cuota generada en la simulación con su desglose financiero</summary>
    public class CuotaSimulada
    {
        /// <summary>Número de la cuota en el plan de pagos</summary>
        public int Numero { get; set; }
        /// <summary>Fecha de vencimiento de la cuota</summary>
        public string Fecha { get; set; } = "";
        /// <summary>Porción de capital de la cuota</summary>
        public decimal Capital { get; set; }
        /// <summary>Porción de interés de la cuota</summary>
        public decimal Interes { get; set; }
        /// <summary>Valor total de la cuota (capital + interés)</summary>
        public decimal Total { get; set; }
        /// <summary>Saldo pendiente después de pagar esta cuota</summary>
        public decimal Saldo { get; set; }
    }
}
