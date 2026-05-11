using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using AppPrestamos.Data;
using AppPrestamos.Enums;
using AppPrestamos.Models;
using AppPrestamos.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace AppPrestamos.ViewModels
{
    public partial class ClientesViewModel : ObservableValidator
    {
        [ObservableProperty]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres")]
        private string nombre = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "La cédula es obligatoria")]
        [MinLength(6, ErrorMessage = "La cédula debe tener al menos 6 caracteres")]
        private string cedula = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        private string telefono = string.Empty;

        [ObservableProperty]
        private string direccion = string.Empty;

        [ObservableProperty]
        private Cliente? clienteSeleccionado;

        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        [ObservableProperty]
        private bool estaEditando;

        [ObservableProperty]
        private string errorFormulario = string.Empty;

        private int? clienteIdEditando;

        public bool TieneError => !string.IsNullOrEmpty(ErrorFormulario);
        public string TextoBotonGuardar => EstaEditando ? "Actualizar Cliente" : "Guardar Cliente";
        private int totalClientesDb;

        public string TotalClientes => $"{totalClientesDb} registrados";

        public ObservableCollection<Cliente> Clientes { get; } = new();

        public ClientesViewModel()
        {
            CargarClientes();
        }

        partial void OnTextoBusquedaChanged(string value) => CargarClientes();
        partial void OnErrorFormularioChanged(string value) => OnPropertyChanged(nameof(TieneError));
        partial void OnEstaEditandoChanged(bool value) => OnPropertyChanged(nameof(TextoBotonGuardar));

        private void CargarClientes()
        {
            using var db = new AppDbContext();
            totalClientesDb = db.Clientes.Count();
            OnPropertyChanged(nameof(TotalClientes));

            Clientes.Clear();
            var query = db.Clientes.AsQueryable();
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                var busqueda = TextoBusqueda.ToLower();
                query = query.Where(c => c.Nombre.ToLower().Contains(busqueda)
                    || c.Cedula.Contains(busqueda)
                    || c.Telefono.Contains(busqueda));
            }
            foreach (var c in query.OrderBy(c => c.Nombre))
                Clientes.Add(c);
        }

        [RelayCommand]
        private void Guardar()
        {
            ErrorFormulario = string.Empty;
            ValidateAllProperties();
            if (HasErrors) return;

            using var db = new AppDbContext();

            if (db.Clientes.Any(c => c.Cedula == Cedula && c.Id != clienteIdEditando))
            {
                ErrorFormulario = "Ya existe un cliente con esa cédula.";
                return;
            }

            if (EstaEditando && clienteIdEditando.HasValue)
            {
                var cliente = db.Clientes.Find(clienteIdEditando.Value);
                if (cliente is null) return;
                cliente.Nombre = Nombre;
                cliente.Cedula = Cedula;
                cliente.Telefono = Telefono;
                cliente.Direccion = Direccion;
                db.SaveChanges();
                new AuditService().Registrar("Actualizar", "Cliente", cliente.Id,
                    $"Cliente '{cliente.Nombre}' actualizado.");
                MessageBox.Show("Cliente actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var cliente = new Cliente
                {
                    Nombre = Nombre,
                    Cedula = Cedula,
                    Telefono = Telefono,
                    Direccion = Direccion
                };
                db.Clientes.Add(cliente);
                db.SaveChanges();
                new AuditService().Registrar("Crear", "Cliente", cliente.Id,
                    $"Cliente '{cliente.Nombre}' creado.");
                MessageBox.Show("Cliente guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            CancelarEdicion();
            CargarClientes();
        }

        [RelayCommand]
        private void Editar()
        {
            if (ClienteSeleccionado is null)
            {
                MessageBox.Show("Seleccione un cliente de la lista.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            clienteIdEditando = ClienteSeleccionado.Id;
            Nombre = ClienteSeleccionado.Nombre;
            Cedula = ClienteSeleccionado.Cedula;
            Telefono = ClienteSeleccionado.Telefono;
            Direccion = ClienteSeleccionado.Direccion;
            EstaEditando = true;
            ErrorFormulario = string.Empty;
        }

        [RelayCommand]
        private void CancelarEdicion()
        {
            clienteIdEditando = null;
            Nombre = string.Empty;
            Cedula = string.Empty;
            Telefono = string.Empty;
            Direccion = string.Empty;
            EstaEditando = false;
            ErrorFormulario = string.Empty;
        }

        [RelayCommand]
        private void Eliminar()
        {
            if (ClienteSeleccionado is null)
            {
                MessageBox.Show("Seleccione un cliente de la lista.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"¿Eliminar a {ClienteSeleccionado.Nombre}? Esta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();
            var cliente = db.Clientes.Include(c => c.Prestamos).FirstOrDefault(c => c.Id == ClienteSeleccionado.Id);
            if (cliente is null) return;

            if (cliente.Prestamos.Any(p => p.Estado == EstadoPrestamo.Activo || p.Estado == EstadoPrestamo.EnMora))
            {
                MessageBox.Show("No se puede eliminar el cliente porque tiene préstamos activos o en mora.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            db.Clientes.Remove(cliente);
            db.SaveChanges();
            new AuditService().Registrar("Eliminar", "Cliente", cliente.Id,
                $"Cliente '{cliente.Nombre}' eliminado.");

            ClienteSeleccionado = null;
            CargarClientes();
            MessageBox.Show("Cliente eliminado correctamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
