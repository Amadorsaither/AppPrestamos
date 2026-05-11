using System.Collections.ObjectModel;
using AppPrestamos.Data;
using Microsoft.EntityFrameworkCore;
using AppPrestamos.Enums;
using AppPrestamos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppPrestamos.ViewModels
{
    public partial class CuotasViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Prestamo> prestamos = new();

        [ObservableProperty]
        private ObservableCollection<Cuota> cuotas = new();

        [ObservableProperty]
        private Prestamo? prestamoSeleccionado;

        [ObservableProperty]
        private Cuota? cuotaSeleccionada;

        [ObservableProperty]
        private string filtroEstado = "Todas";

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

        [RelayCommand]
        private void Recargar()
        {
            CargarPrestamos();
            CargarCuotas();
        }
    }
}
