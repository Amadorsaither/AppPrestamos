# Exposición - Sistema AppPrestamos

---

## 1. Introducción (2 min)

**"Buenas, mi nombre es Amado Saither Amador y les voy a presentar el proyecto AppPrestamos, un sistema de gestión de préstamos financieros desarrollado en C# con WPF."**

### ¿Qué hace?
- Administra clientes, préstamos, cuotas y pagos.
- Genera reportes exportables a PDF y Excel.
- Controla morosidad, respalda la base de datos y registra auditoría.

### Tecnologías
- **Lenguaje:** C# con .NET 8
- **Interfaz:** WPF (Windows Presentation Foundation)
- **Arquitectura:** MVVM (Model-View-ViewModel)
- **Base de datos:** SQLite con Entity Framework Core
- **Gráficos:** LiveChartsCore
- **Reportes:** QuestPDF (PDF) y ClosedXML (Excel)

---

## 2. Arquitectura MVVM (3 min)

**"El proyecto está organizado en tres capas principales:"**

```
AppPrestamos/
├── Models/          -> Entidades de la BD
├── ViewModels/      -> Lógica de cada pantalla
├── Views/           -> Interfaces XAML
├── Services/        -> Lógica de negocio reutilizable
├── Data/            -> DbContext y SeedData
```

### Flujo de navegación

**"La navegación funciona con un ContentControl en MainWindow que cambia su contenido según una propiedad SelectedSection."**

```csharp
   
{
    SelectedSection = section;
    CurrentView = section switch
    {
        "Dashboard" => new DashboardViewModel(),
        "Clientes"  => new ClientesViewModel(),
        "Prestamos" => new PrestamosViewModel(),
        "Pagos"     => new PagosViewModel(),
        _           => new DashboardViewModel()
    };
}
```

**"Cada sección tiene su propio ViewModel y su DataTemplate en App.xaml:"**

```xml
<!-- App.xaml -->
<DataTemplate DataType="{x:Type vm:DashboardViewModel}">
    <views:DashboardView/>
</DataTemplate>
<DataTemplate DataType="{x:Type vm:ClientesViewModel}">
    <views:ClientesView/>
</DataTemplate>
```

> **"Esto permite que WPF resuelva automáticamente qué vista mostrar según el ViewModel asignado."**

### Comunicación entre vistas

**"Para pasar datos entre pantallas, usamos WeakReferenceMessenger:"**

```csharp
// DashboardViewModel.cs - Envía mensaje con ID de cuota
[RelayCommand]
private void IrAPago(ProximoVencimiento vencimiento)
{
    WeakReferenceMessenger.Default.Send(
        new NavigateToPagoConCuotaMessage(vencimiento.CuotaId));
}

// MainViewModel.cs - Recibe y navega
WeakReferenceMessenger.Default.Register<NavigateToPagoConCuotaMessage>(this, (r, m) =>
{
    CurrentView = new PagosViewModel(m.CuotaId);
    SelectedSection = "Pagos";
});
```

---

## 3. Login y Autenticación (2 min)

**"El sistema tiene un login con contraseñas hasheadas usando PBKDF2."**

### Flujo de inicio

```csharp
// App.xaml.cs - OnStartup
protected override void OnStartup(StartupEventArgs e)
{
    var login = new LoginWindow();
    if (login.ShowDialog() != true)
    {
        Application.Current.Shutdown();
        return;
    }
    new MainWindow().Show();
}
```

**"Si el usuario no se autentica correctamente, la aplicación se cierra."**

### Hash de contraseñas

```csharp
// AuthService.cs
public static string HashPassword(string password)
{
    byte[] salt = RandomNumberGenerator.GetBytes(16);
    byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
        password, salt, 100000, HashAlgorithmName.SHA256, 32);
    return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
}
```

> **"Usamos 100,000 iteraciones de SHA256 con sal aleatoria de 16 bytes."**

---

## 4. Dashboard (3 min)

**"El Dashboard es la pantalla principal con 4 secciones:"**

### Tarjetas de resumen

```csharp
// DashboardViewModel.cs - CargarDatos()
var totalClientesDb = db.Clientes.Count();
TotalClientes = totalClientesDb.ToString("N0");

var prestamosActivos = db.Prestamos.Count(p => p.Estado == EstadoPrestamo.Activo);
TotalPrestamos = prestamosActivos.ToString("N0");

var montoTotal = db.Prestamos.AsEnumerable().Sum(p => p.Monto);
TotalMonto = $"${montoTotal:N2}";

var pagosMes = db.Pagos
    .Where(p => p.FechaPago.Year == hoy.Year && p.FechaPago.Month == hoy.Month)
    .AsEnumerable().Sum(p => p.MontoPagado);
TotalPagos = $"${pagosMes:N2}";
```

> **"Noten que usamos .AsEnumerable().Sum() porque SQLite no soporta decimal.Sum() directamente en LINQ."**

### Gráfico de dona

```csharp
var verde = SKColor.Parse("#10B981");
var azul = SKColor.Parse("#3B82F6");
var rojo = SKColor.Parse("#EF4444");

SeriesEstados = new ISeries[]
{
    new PieSeries<double> {
        Values = [activos], Name = "Activos",
        Fill = new SolidColorPaint(verde),
        Stroke = new SolidColorPaint(SKColors.White, 3),
        HoverPushout = 8, InnerRadius = 65
    },
    new PieSeries<double> {
        Values = [pagados], Name = "Pagados",
        Fill = new SolidColorPaint(azul),
        Stroke = new SolidColorPaint(SKColors.White, 3),
        HoverPushout = 8, InnerRadius = 65
    },
    new PieSeries<double> {
        Values = [mora], Name = "En Mora",
        Fill = new SolidColorPaint(rojo),
        Stroke = new SolidColorPaint(SKColors.White, 3),
        HoverPushout = 8, InnerRadius = 65
    }
};
```

> **"El Stroke blanco de 3px separa los segmentos para un look más limpio."**

### Notificaciones

**"Las notificaciones se generan desde la base de datos consultando cuotas vencidas, próximas a vencer, pagos del día y clientes nuevos. Al hacer clic, se marcan como vistas en un HashSet y navegan a la sección correspondiente."**

```csharp
private static readonly HashSet<string> NotificacionesVistas = new();

[RelayCommand]
private void NotificacionClick(NotificacionItem item)
{
    NotificacionesVistas.Add(item.ItemKey);
    Notificaciones.Remove(item);
    // navegar según tipo...
}
```

---

## 5. Clientes - CRUD (2 min)

**"La vista de clientes tiene un buscador en tiempo real:"**

```csharp
// ClientesViewModel.cs
[RelayCommand]
private void Buscar()
{
    using var db = new AppDbContext();
    var query = db.Clientes.AsQueryable();

    if (!string.IsNullOrWhiteSpace(TextoBusqueda))
        query = query.Where(c =>
            c.Nombre.Contains(TextoBusqueda) ||
            c.Apellido.Contains(TextoBusqueda) ||
            c.Telefono.Contains(TextoBusqueda));

    Clientes = new ObservableCollection<Cliente>(query.ToList());
    TotalClientes = query.Count();
}
```

**"El TextBox de búsqueda está bindeado con UpdateSourceTrigger=PropertyChanged para que filtre en cada tecla:"**

```xml
<TextBox Text="{Binding TextoBusqueda, UpdateSourceTrigger=PropertyChanged}"
         PlaceholderText="Buscar por nombre, apellido o teléfono..."/>
```

---

## 6. Préstamos y Cuotas (3 min)

**"Al crear un préstamo, las cuotas se generan automáticamente:"**

```csharp
// PrestamoService.cs
public static List<Cuota> GenerarCuotas(Prestamo prestamo, int numeroCuotas, decimal tasaAnual)
{
    var cuotas = new List<Cuota>();
    decimal tasaMensual = tasaAnual / 12 / 100;
    decimal montoCuota = (prestamo.Monto * tasaMensual *
        (decimal)Math.Pow(1 + (double)tasaMensual, numeroCuotas)) /
        ((decimal)Math.Pow(1 + (double)tasaMensual, numeroCuotas) - 1);

    for (int i = 1; i <= numeroCuotas; i++)
    {
        cuotas.Add(new Cuota
        {
            PrestamoId = prestamo.Id,
            NumeroCuota = i,
            FechaVencimiento = prestamo.FechaInicio.AddMonths(i),
            MontoCuota = Math.Round(montoCuota, 2),
            SaldoPendiente = Math.Round(montoCuota, 2),
            Estado = EstadoCuota.Pendiente
        });
    }
    return cuotas;
}
```

> **"Usamos el método de amortización francés (cuota fija) con interés compuesto."**

### Vista de cuotas con filtro

```csharp
// CuotasViewModel.cs
[RelayCommand]
private void Filtrar()
{
    using var db = new AppDbContext();
    var query = db.Cuotas.Include(c => c.Prestamo).ThenInclude(p => p.Cliente).AsQueryable();

    if (FiltroEstado != "Todos")
        query = query.Where(c => c.Estado.ToString() == FiltroEstado);

    if (PrestamoId.HasValue)
        query = query.Where(c => c.PrestamoId == PrestamoId);

    Cuotas = new ObservableCollection<ListadoCuota>(query
        .Select(c => new ListadoCuota { /* mapeo */ }).ToList());
}
```

---

## 7. Pagos (2 min)

**"El registro de pagos tiene una característica interesante: al seleccionar una cuota, el monto se auto-completa con el saldo pendiente."**

```csharp
// PagosViewModel.cs
private void OnCuotaSeleccionadaIdChanged(int? value)
{
    if (value.HasValue)
    {
        using var db = new AppDbContext();
        var cuota = db.Cuotas.Find(value.Value);
        if (cuota != null)
        {
            MontoPago = cuota.SaldoPendiente;
            // Mostrar detalle de la cuota
        }
    }
}
```

**"El ComboBox usa SelectedValue en lugar de SelectedItem para evitar problemas de igualdad por referencia:"**

```xml
<ComboBox ItemsSource="{Binding CuotasPendientes}"
          SelectedValue="{Binding CuotaSeleccionadaId}"
          SelectedValuePath="Id"
          DisplayMemberPath="DisplayText"/>
```

**"Al eliminar un pago, el saldo de la cuota se restaura automáticamente:"**

```csharp
[RelayCommand]
private async Task EliminarPago(Pago pago)
{
    using var db = new AppDbContext();
    var cuota = db.Cuotas.Find(pago.CuotaId);
    cuota.SaldoPendiente += pago.MontoPagado;
    cuota.Estado = EstadoCuota.Parcial;
    db.Pagos.Remove(pago);
    await db.SaveChangesAsync();
    CargarDatos();
}
```

---

## 8. Reportes Exportables (2 min)

**"La sección de reportes tiene 6 indicadores clave más tres visualizaciones:"**

### Exportación a PDF

```csharp
// ReportesService.cs
public static byte[] GenerarReporteMoraPDF(List<ClienteMora> mora)
{
    QuestPDF.Settings.License = LicenseType.Community;

    var pdf = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.Header().Text("Reporte de Morosidad")
                .SemiBold().FontSize(20).FontColor("#0F172A");

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                foreach (var item in mora)
                {
                    table.Cell().Text(item.Cliente);
                    table.Cell().Text($"${item.Monto:N2}");
                    table.Cell().Text($"{item.DiasAtraso} días");
                }
            });
        });
    });
    return pdf.GeneratePdf();
}
```

### Exportación a Excel

```csharp
using (var workbook = new XLWorkbook())
{
    var ws = workbook.Worksheets.Add("Reporte");
    ws.Cell(1, 1).Value = "Cliente";
    ws.Cell(1, 2).Value = "Monto";
    ws.Cell(1, 3).Value = "Días";

    int row = 2;
    foreach (var item in datos)
    {
        ws.Cell(row, 1).Value = item.Cliente;
        ws.Cell(row, 2).Value = item.Monto;
        ws.Cell(row, 3).Value = item.DiasAtraso;
        row++;
    }

    workbook.SaveAs(ruta);
}
```

---

## 9. Auditoría (1 min)

**"Cada operación importante queda registrada en la tabla de auditoría:"**

```csharp
// AuditService.cs
public static void Registrar(string accion, string entidad, string detalle)
{
    using var db = new AppDbContext();
    db.Auditoria.Add(new Auditoria
    {
        Usuario = App.UsuarioActual?.NombreUsuario ?? "Sistema",
        Accion = accion,
        Entidad = entidad,
        Detalle = detalle,
        Fecha = DateTime.Now
    });
    db.SaveChanges();
}
```

**"Se llama desde cada ViewModel después de crear, modificar o eliminar:"**

```csharp
AuditService.Registrar("Crear", "Cliente", $"Cliente {cliente.Nombre} creado");
```

---

## 10. Respaldo y Restauración (1 min)

**"El sistema permite respaldar y restaurar la base de datos con un solo clic:"**

```csharp
// BackupService.cs
public static void RealizarRespaldo(string rutaDestino)
{
    string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prestamos.db");
    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    string archivo = Path.Combine(rutaDestino, $"respaldo_{timestamp}.db");
    File.Copy(dbPath, archivo, true);
}
```

**"La restauración simplemente reemplaza el archivo actual por el respaldo seleccionado."**

---

## 11. Simulador de Préstamos (1 min)

**"El simulador permite calcular cuotas antes de crear un préstamo:"**

```csharp
// SimuladorViewModel.cs
[RelayCommand]
private void Calcular()
{
    var prestamoSimulado = new Prestamo
    {
        Monto = MontoSimulado,
        FechaInicio = DateTime.Today,
        InteresAnual = TasaInteres
    };

    CuotasSimuladas = new ObservableCollection<Cuota>(
        PrestamoService.GenerarCuotas(prestamoSimulado, NumeroCuotas, TasaInteres)
    );
}
```

---

## 12. Calendario de Pagos (1 min)

**"El calendario muestra un grid mensual con los pagos registrados:"**

```csharp
// CalendarioViewModel.cs
public void CargarMes(int year, int month)
{
    Dias = new ObservableCollection<DiaCalendario>();
    int diasEnMes = DateTime.DaysInMonth(year, month);

    for (int dia = 1; dia <= diasEnMes; dia++)
    {
        var fecha = new DateTime(year, month, dia);
        var pagosDelDia = db.Pagos.Count(p =>
            p.FechaPago.Year == year &&
            p.FechaPago.Month == month &&
            p.FechaPago.Day == dia);

        Dias.Add(new DiaCalendario
        {
            Numero = dia,
            TienePago = pagosDelDia > 0,
            CantidadPagos = pagosDelDia
        });
    }
}
```

---

## 13. Demostración (5 min)

**"Ahora voy a hacer un recorrido rápido por el sistema:"**

1. **Login** - Ingreso con admin/admin123
2. **Dashboard** - Muestro las tarjetas, el gráfico de dona, los vencimientos y las notificaciones
3. **Clientes** - Busco un cliente por nombre, lo edito
4. **Préstamos** - Creo un préstamo nuevo, muestro las cuotas generadas
5. **Pagos** - Selecciono una cuota, registro un pago, muestro cómo se actualiza el saldo
6. **Reportes** - Genero un PDF de mora, muestro el gráfico de ingresos
7. **Auditoría** - Filtro por fecha para ver los registros
8. **Respaldo** - Hago una copia de seguridad
9. **Simulador** - Calculo cuotas para un préstamo hipotético
10. **Calendario** - Navego entre meses para ver pagos

---

## 14. Preguntas frecuentes (para la ronda de preguntas)

### ¿Por qué SQLite y no SQL Server?
**"Elegí SQLite porque es embebido, no requiere instalación de servidor y el archivo se puede transportar fácilmente. Para un sistema de este tamaño es más que suficiente."**

### ¿Cómo se maneja la seguridad?
**"Las contraseñas se almacenan con hash PBKDF2 con sal aleatoria. No se almacenan en texto plano. Además, cada operación crítica queda registrada en auditoría."**

### ¿Se puede usar en red?
**"Actualmente es monousuario. Se podría migrar a SQL Server y agregar un servicio WCF o API REST para trabajo en red."**

### ¿Qué pasa si se cierra el programa mientras se hace un pago?
**"Cada operación de base de datos se ejecuta dentro de una transacción de EF Core. Si algo falla, los cambios no se persisten."**

---
