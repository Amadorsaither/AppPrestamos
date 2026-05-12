namespace AppPrestamos.Enums
{
    /// <summary>Define los estados posibles de una cuota</summary>
    public enum EstadoCuota
    {
        /// <summary>Cuota pendiente de pago</summary>
        Pendiente = 1,
        /// <summary>Cuota con pago parcial</summary>
        Parcial = 2,
        /// <summary>Cuota pagada en su totalidad</summary>
        Pagada = 3,
        /// <summary>Cuota vencida y no pagada</summary>
        Vencida = 4
    }
}