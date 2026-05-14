# Reporte de Uso de Inteligencia Artificial

## Datos del Proyecto

- **Nombre del proyecto:** AppPrestamos
- **Autor:** Amado Saither Amador
- **Tecnologías utilizadas:** C#, WPF, .NET 8, MVVM, Entity Framework Core, SQLite, LiveChartsCore, QuestPDF, ClosedXML

---

## Herramienta de Inteligencia Artificial Utilizada

Se utilizó **opencode y Open Ai Chap Gpt** (modelo big-pickle) como herramienta de asistencia técnica durante el desarrollo del proyecto. Su función fue equivalente a la de un programador junior que recibe instrucciones detalladas y ejecuta tareas específicas bajo supervisión constante.

---

## Actividades en las que se utilizó IA

La inteligencia artificial se empleó como asistencia técnica en las siguientes áreas:

### 1. Diseño de la Interfaz Gráfica (WPF)
- Creación del layout del Dashboard con tarjetas resumen, gráficos y panel de notificaciones.
- Diseño del login con toggle de visibilidad de contraseña (ojo).
- Sidebar con navegación por secciones y resaltado visual de la sección activa mediante DataTriggers.
- Header con avatar de usuario, inicial y menú de perfil/cierre de sesión.
- Tarjetas de reportes con donut, barras mensuales, tabla de mora con colores por días de atraso.
- Calendario de pagos mensual con indicadores visuales por estado.
- Perfil de usuario con cambio de contraseña.
- Estilo moderno con sombras suaves, bordes redondeados y paleta de colores definida por el autor.

### 2. Arquitectura MVVM
- Organización del proyecto en Models, Views, ViewModels, Services y Data.
- Uso de `RelayCommand` (CommunityToolkit.Mvvm) para todos los comandos.
- Navegación entre vistas mediante `ContentControl` + `DataTemplates` en `App.xaml`.
- Comunicación entre ViewModels usando `WeakReferenceMessenger`.
- Inyección de dependencia manual y sesión de usuario via `App.UsuarioActual`.

### 3. Funcionalidades del Sistema
- **Login y autenticación:** Hash de contraseñas con PBKDF2 (Rfc2898DeriveBytes), tabla Usuario creada automáticamente, credencial por defecto admin/admin123.
- **Dashboard:** 4 tarjetas de resumen (clientes, préstamos activos, monto total, pagos del mes), donut de estados con leyenda numérica, próximos vencimientos con navegación directa a pagos, préstamos recientes, panel de notificaciones interactivas.
- **CRUD de Clientes:** Lista con buscador en tiempo real.
- **Préstamos:** Creación con generación automática de cuotas, validaciones, cancelación y eliminación.
- **Cuotas:** Filtro por estado con DataGrid mostrando capital, interés, mora, total y saldo.
- **Pagos:** Registro con auto-completado de saldo, eliminación con restauración de saldo, ComboBox con formato detallado.
- **Reportes:** 6 indicadores, donut de estados, barras de ingresos mensuales, tabla de morosidad con días coloreados, resumen mensual. Exportación a PDF (QuestPDF) y Excel (ClosedXML).
- **Auditoría:** Registro automático de operaciones CRUD con DataGrid filtrable.
- **Respaldo y restauración:** Copia de seguridad de la base de datos con timestamp.
- **Simulador de préstamos:** Tabla de amortición completa con interés simple/compuesto.
- **Calendario de pagos:** Grid mensual con indicadores por día.
- **Notificaciones:** Generación desde BD (cuotas vencidas, próximas, parciales, nuevos préstamos, pagos y clientes del día), clic elimina y navega a la sección correspondiente.

### 4. Base de Datos
- Integración con Entity Framework Core + SQLite.
- Configuración del `AppDbContext` con todas las entidades.
- Población automática con datos de prueba mediante `SeedData.cs` (30 clientes, ~1-3 préstamos cada uno con cuotas y pagos realistas).

### 5. Exportación de Reportes
- Generación de archivos PDF con QuestPDF y licencia Community.
- Exportación a Excel con ClosedXML.

### 6. Control de Versiones
- Publicación del repositorio en GitHub.
- Configuración de `.gitignore` para excluir archivos de publicación.
- Creación de release v1.0.

### 7. Documentación
- Elaboración del archivo `README.md` con características, tecnologías, instalación y compilación.
- Documentación XML en todos los archivos `.cs`.
- Redacción de este reporte de uso de inteligencia artificial.

---

## Aporte Personal del Autor

Todo el código generado con ayuda de la inteligencia artificial fue:

- **Revisado manualmente** línea por línea antes de ser aceptado.
- **Adaptado** a los requerimientos específicos del proyecto.
- **Integrado y probado** por el autor.
- **Modificado y corregido** cuando no cumplía con las expectativas.
- **Validado** mediante compilación y ejecución.

Las decisiones arquitectónicas (como el uso de `SelectedValue` en lugar de `SelectedItem` para los ComboBox, el uso de `HashSet` para notificaciones vistas, el flujo de login sin `StartupUri`, el cálculo de cuotas con interés simple/compuesto, y la estructura general del proyecto) fueron tomadas por el autor.

La inteligencia artificial se limitó a proponer implementaciones basadas en instrucciones precisas, y el autor determinó qué aceptar, qué modificar y qué descartar.

---

## Beneficios Obtenidos

El uso de inteligencia artificial permitió:

- Reducir el tiempo de escritura de código repetitivo (DataGrids, formularios, estilos XAML).
- Obtener ejemplos de sintaxis específica de librerías (LiveChartsCore, QuestPDF, ClosedXML).
- Resolver errores de compilación y configuración de paquetes NuGet.
- Mantener consistencia en la documentación XML.
- Agilizar la creación del archivo de datos de prueba (SeedData).

---

## Conclusión

La inteligencia artificial se utilizó como herramienta de asistencia técnica bajo la supervisión y dirección constante del autor. El desarrollo, la arquitectura, las decisiones de diseño, la lógica de negocio, la validación y las correcciones fueron realizadas por **Amado Saither Amador**. La IA actuó como un asistente que ejecutó tareas específicas indicadas paso a paso, sin autonomía ni capacidad de decisión sobre el rumbo del proyecto.

---

## Firma

**Amado Saither Amador**
