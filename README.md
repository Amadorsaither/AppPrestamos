# AppPrestamos

Sistema de gestión de préstamos financieros desarrollado en C# con WPF y patrón MVVM. Interfaz moderna estilo SaaS con dashboard interactivo, CRUD completo, reportes exportables y más.

## Capturas

*(pendiente)*

## Características

### Módulos Principales
- **Dashboard** — tarjetas resumen, sparklines de tendencia, gráfico donut de estados, barras de frecuencia, próximos vencimientos y préstamos recientes
- **Clientes** — CRUD completo con buscador en tiempo real, validaciones y total de registros
- **Préstamos** — registro con cálculo automático de cuotas, selección de tipo de interés (simple/compuesto) y frecuencia de pago (semanal/quincenal/mensual), control de estado (Activo/Pagado/En Mora)
- **Cuotas** — filtro por estado (Todas/Pendiente/Pagada/Vencida), tabla con capital, interés, mora, total y saldo pendiente
- **Pagos** — registro de pagos con auto-cálculo de saldo, historial de últimos 50 pagos, eliminación con restauración de saldo

### Reportes y Exportación
- **Reportes** — 6 tarjetas resumen, donut de distribución, barras de 12 meses, tabla de mora con códigos de color por días de vencimiento, resumen mensual
- **Exportación PDF** (QuestPDF) — reporte completo con marca de agua Community
- **Exportación Excel** (ClosedXML) — datos estructurados en hojas de cálculo

### Herramientas
- **Simulador** — calcula tabla de amortización completa con monto, tasa, cuotas, tipo de interés y frecuencia
- **Calendario de Pagos** — grid mensual con indicadores visuales de pagos por día, detalle del día seleccionado, navegación entre meses
- **Auditoría** — registro de todas las acciones (crear/actualizar/eliminar) en clientes, préstamos y pagos, con DataGrid filtrable
- **Respaldo y Restauración** — backup de la base de datos con timestamp, restauración que sobrescribe y reinicia la app

### Sistema de Usuarios
- Login con validación y contraseña hasheada (PBKDF2 con SHA256)
- Toggle de visibilidad de contraseña (ojo)
- Sesión persistente mientras la ventana está abierta
- Cierre de sesión con retorno a pantalla de login
- Perfil de usuario con cambio de contraseña

### Interfaz
- Sidebar con navegación y selección visual (destacado azul)
- Avatar con inicial del usuario, nombre y rol
- Diseño responsive 1366×768, bordes redondeados, sombras suaves
- Paleta azul oscuro/blanco/verde/naranja/morado con textos legibles

## Tecnologías

| Tecnología | Uso |
|---|---|
| C# / .NET 8 | Lenguaje y runtime |
| WPF | Interfaz gráfica de escritorio |
| MVVM con CommunityToolkit.Mvvm | Patrón de diseño con source generators |
| Entity Framework Core + SQLite | ORM y base de datos local |
| LiveChartsCore (SkiaSharp) | Gráficos y sparklines |
| QuestPDF | Exportación de reportes PDF |
| ClosedXML | Exportación a Excel |
| PBKDF2 (Rfc2898DeriveBytes) | Hash de contraseñas |

## Instalación

```bash
git clone https://github.com/Amadorsaither/AppPrestamos.git
cd AppPrestamos
dotnet restore
dotnet run
```

Credenciales por defecto: `admin` / `admin123`

## Compilar Ejecutable

**Framework-dependent** (requiere .NET 8 runtime):
```bash
dotnet publish -c Release -r win-x64 --self-contained false -o publish
```

**Self-contained** (no requiere runtime, ~150 MB):
```bash
dotnet publish -c Release -r win-x64 --self-contained true -o publish
```

## Autor

**Saither Amador**

GitHub: [https://github.com/Amadorsaither](https://github.com/Amadorsaither)
