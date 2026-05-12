namespace AppPrestamos.Enums
{
    /// <summary>Define el tipo de interés aplicable a un préstamo</summary>
    public enum TipoInteres
    {
        /// <summary>Interés simple calculado sobre el capital inicial</summary>
        Simple = 1,
        /// <summary>Interés compuesto calculado sobre capital más intereses acumulados</summary>
        Compuesto = 2
    }
}