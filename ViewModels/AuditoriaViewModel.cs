using System.Collections.ObjectModel;
using System.Windows.Input;
using AppPrestamos.Data;
using AppPrestamos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppPrestamos.ViewModels
{
    public partial class AuditoriaViewModel : ObservableObject
    {
        [ObservableProperty]
        private string textoBusqueda = "";

        [ObservableProperty]
        private string filtroAccion = "Todas";

        [ObservableProperty]
        private string filtroEntidad = "Todas";

        public ObservableCollection<Auditoria> Registros { get; } = [];
        public ObservableCollection<string> Acciones { get; } = ["Todas", "Crear", "Actualizar", "Eliminar"];
        public ObservableCollection<string> Entidades { get; } = ["Todas", "Cliente", "Prestamo", "Cuota", "Pago"];

        public ICommand RecargarCommand { get; }

        public AuditoriaViewModel()
        {
            RecargarCommand = new RelayCommand(CargarRegistros);
            CargarRegistros();
        }

        partial void OnTextoBusquedaChanged(string value) => CargarRegistros();

        private void CargarRegistros()
        {
            using var db = new AppDbContext();

            var query = db.Auditorias.AsQueryable();

            if (FiltroAccion != "Todas")
                query = query.Where(a => a.Accion == FiltroAccion);

            if (FiltroEntidad != "Todas")
                query = query.Where(a => a.Entidad == FiltroEntidad);

            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                var busqueda = TextoBusqueda.ToLower();
                query = query.Where(a =>
                    a.UsuarioNombre.ToLower().Contains(busqueda) ||
                    a.Detalle.ToLower().Contains(busqueda) ||
                    a.Entidad.ToLower().Contains(busqueda));
            }

            Registros.Clear();
            foreach (var r in query.OrderByDescending(a => a.Fecha).Take(200))
                Registros.Add(r);
        }
    }
}
