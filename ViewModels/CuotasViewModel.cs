using System.Collections.ObjectModel;
using AppPrestamos.Data;
using Microsoft.EntityFrameworkCore;
using AppPrestamos.Enums;
using AppPrestamos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppPrestamos.ViewModels
{
    /// <summary>ViewModel para la visualización y filtrado de cuotas por préstamo</summary>
    public partial class CuotasViewModel : ObservableObject
    {
        /// <summary>Colección observable de préstamos para seleccionar</summary>
        [ObservableProperty]
        private ObservableCollection<Prestamo> prestamos = new();

        /// <summary>Colección observable de cuotas del préstamo seleccionado</summary>
        [ObservableProperty]
        private ObservableCollection<Cuota> cuotas = new();

        /// <summary>Préstamo seleccionado para ver sus cuotas</summary>
        [ObservableProperty]
        private Prestamo? prestamoSeleccionado;

        /// <summary>Cuota seleccionada en la lista</summary>
        [ObservableProperty]
        private Cuota? cuotaSeleccionada;

        /// <summary>Texto del filtro de estado de cuotas</summary>
        [ObservableProperty]
        private string filtroEstado = "Todas";

        /// <summary>Opciones disponibles para filtrar cuotas por estado</summary>
        public string[] EstadosFiltro { get; } = { "Todas", "Pendiente", "Pagada", "Vencida" };

        public CuotasViewModel()
        {
            CargarPrestamos();
        }

        partial void OnPrestamoSeleccionadoChanged(Prestamo? value)
        {
            CargarCuotas();
        }

        partial void OnFiltroEstadoChanged(string value)
        {
            CargarCuotas();
        }

        private void CargarPrestamos()
        {
            using var db = new AppDbContext();
            Prestamos.Clear();
            foreach (var p in db.Prestamos.Include("Cliente").OrderByDescending(p => p.FechaInicio))
                Prestamos.Add(p);
        }

        private void CargarCuotas()
        {
            Cuotas.Clear();
            if (PrestamoSeleccionado is null) return;

            using var db = new AppDbContext();
            var query = db.Cuotas.Where(c => c.PrestamoId == PrestamoSeleccionado.Id);

            if (FiltroEstado == "Pendiente")
                query = query.Where(c => c.Estado == EstadoCuota.Pendiente);
            else if (FiltroEstado == "Pagada")
                query = query.Where(c => c.Estado == EstadoCuota.Pagada);
            else if (FiltroEstado == "Vencida")
                query = query.Where(c => c.Estado == EstadoCuota.Vencida);

            foreach (var c in query.OrderBy(c => c.NumeroCuota))
                Cuotas.Add(c);
        }

        /// <summary>Recarga la lista de préstamos y cuotas desde la base de datos</summary>
        [RelayCommand]
        private void Recargar()
        {
            CargarPrestamos();
            CargarCuotas();
        }
    }
}
