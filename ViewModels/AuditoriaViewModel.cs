using System.Collections.ObjectModel;
using System.Windows.Input;
using AppPrestamos.Data;
using AppPrestamos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppPrestamos.ViewModels
{
    /// <summary>ViewModel para la sección de auditoría, permite consultar y filtrar registros de actividad</summary>
    public partial class AuditoriaViewModel : ObservableObject
    {
        /// <summary>Texto de búsqueda para filtrar registros de auditoría</summary>
        [ObservableProperty]
        private string textoBusqueda = "";

        /// <summary>Filtro por tipo de acción (Crear, Actualizar, Eliminar, Todas)</summary>
        [ObservableProperty]
        private string filtroAccion = "Todas";

        /// <summary>Filtro por tipo de entidad (Cliente, Préstamo, Cuota, Pago, Todas)</summary>
        [ObservableProperty]
        private string filtroEntidad = "Todas";

        /// <summary>Colección de registros de auditoría filtrados</summary>
        public ObservableCollection<Auditoria> Registros { get; } = [];
        /// <summary>Lista de acciones disponibles para filtrar</summary>
        public ObservableCollection<string> Acciones { get; } = ["Todas", "Crear", "Actualizar", "Eliminar"];
        /// <summary>Lista de entidades disponibles para filtrar</summary>
        public ObservableCollection<string> Entidades { get; } = ["Todas", "Cliente", "Prestamo", "Cuota", "Pago"];

        /// <summary>Comando para recargar los registros de auditoría aplicando los filtros actuales</summary>
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
