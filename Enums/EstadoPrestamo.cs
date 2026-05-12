namespace AppPrestamos.Enums
{
    /// <summary>Define los estados posibles de un préstamo</summary>
    public enum EstadoPrestamo
    {
        /// <summary>Préstamo activo y en curso</summary>
        Activo = 1,
        /// <summary>Préstamo pagado en su totalidad</summary>
        Pagado = 2,
        /// <summary>Préstamo con cuotas en mora</summary>
        EnMora = 3
    }
}