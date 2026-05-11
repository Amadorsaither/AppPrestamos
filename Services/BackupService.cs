using System.IO;

namespace AppPrestamos.Services
{
    public class BackupService
    {
        private readonly string _rutaDb;

        public BackupService()
        {
            _rutaDb = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prestamos.db");
        }

        public void CrearBackup(string rutaDestino)
        {
            if (!File.Exists(_rutaDb))
                throw new FileNotFoundException("La base de datos no existe.", _rutaDb);

            File.Copy(_rutaDb, rutaDestino, true);
        }

        public void RestaurarBackup(string rutaOrigen)
        {
            if (!File.Exists(rutaOrigen))
                throw new FileNotFoundException("El archivo de respaldo no existe.", rutaOrigen);

            File.Copy(rutaOrigen, _rutaDb, true);
        }

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

        public DateTime? ObtenerFechaBackup(string ruta)
        {
            if (!File.Exists(ruta))
                return null;
            return File.GetLastWriteTime(ruta);
        }
    }
}
