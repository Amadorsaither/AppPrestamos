using System.IO;

namespace AppPrestamos.Services
{
    /// <summary>Servicio de respaldo y restauración de la base de datos SQLite</summary>
    public class BackupService
    {
        private readonly string _rutaDb;

        /// <summary>Inicializa el servicio con la ruta de la base de datos</summary>
        public BackupService()
        {
            _rutaDb = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prestamos.db");
        }

        /// <summary>Crea una copia de respaldo de la base de datos en la ruta especificada</summary>
        public void CrearBackup(string rutaDestino)
        {
            if (!File.Exists(_rutaDb))
                throw new FileNotFoundException("La base de datos no existe.", _rutaDb);

            File.Copy(_rutaDb, rutaDestino, true);
        }

        /// <summary>Restaura la base de datos desde un archivo de respaldo</summary>
        public void RestaurarBackup(string rutaOrigen)
        {
            if (!File.Exists(rutaOrigen))
                throw new FileNotFoundException("El archivo de respaldo no existe.", rutaOrigen);

            File.Copy(rutaOrigen, _rutaDb, true);
        }

        /// <summary>Obtiene el tamaño actual de la base de datos en formato legible (B, KB, MB)</summary>
        public string ObtenerTamanoDb()
        {
            if (!File.Exists(_rutaDb))
                return "0 KB";

            var info = new FileInfo(_rutaDb);
            return info.Length switch
            {
                < 1024 => $"{info.Length} B",
                < 1048576 => $"{info.Length / 1024.0:F1} KB",
                _ => $"{info.Length / 1048576.0:F2} MB"
            };
        }

        /// <summary>Obtiene la fecha de última modificación de un archivo de respaldo</summary>
        public DateTime? ObtenerFechaBackup(string ruta)
        {
            if (!File.Exists(ruta))
                return null;
            return File.GetLastWriteTime(ruta);
        }
    }
}
