using AgileFlow.Domain.Common;

namespace AgileFlow.Domain.Services;

/// <summary>
/// Calcula la posición fraccional de una tarea al reordenarla dentro de una
/// columna (o al moverla a otra), usando "fractional indexing" (a.k.a.
/// LexoRank / gap-based ordering):
///
///   - Insertar al inicio: mitad de la posición del primer elemento.
///   - Insertar al final: última posición + un salto fijo (GapStep).
///   - Insertar entre A y B: punto medio (A + B) / 2.
///
/// Ventaja frente a un índice entero secuencial: mover una tarea no exige
/// reescribir el "order" de todas las demás filas de la columna, solo el de
/// la tarea movida — una sola escritura en base de datos por movimiento,
/// clave para que la propagación en tiempo real sea rápida.
///
/// Si el hueco entre dos posiciones se agota (colisión), se lanza
/// DomainException indicando que la columna requiere un rebalanceo
/// (reasignación de posiciones en bloque, operación infrecuente).
/// </summary>
public static class TaskOrderingService
{
    /// <summary>
    /// Salto por defecto entre posiciones consecutivas cuando no hay
    /// vecino en una de las direcciones (inicio o final de la lista).
    /// </summary>
    public const decimal GapStep = 1024m;

    /// <summary>
    /// Menor diferencia aceptable entre dos posiciones antes de considerar
    /// que el hueco se agotó y se requiere rebalanceo.
    /// </summary>
    private const decimal MinimumGap = 0.0001m;

    /// <summary>
    /// Calcula la nueva posición de una tarea dados sus nuevos vecinos
    /// (la tarea que queda inmediatamente antes y la que queda inmediatamente
    /// después, en la columna destino, ya sin contar la tarea que se mueve).
    /// </summary>
    /// <param name="previousPosition">Posición del vecino anterior, o null si la tarea queda de primera.</param>
    /// <param name="nextPosition">Posición del vecino siguiente, o null si la tarea queda de última.</param>
    public static decimal CalculateNewPosition(decimal? previousPosition, decimal? nextPosition)
    {
        // Columna vacía: primera tarea.
        if (previousPosition is null && nextPosition is null)
            return GapStep;

        // Se inserta al inicio de la lista.
        if (previousPosition is null)
            return nextPosition!.Value / 2m;

        // Se inserta al final de la lista.
        if (nextPosition is null)
            return previousPosition.Value + GapStep;

        if (previousPosition.Value >= nextPosition.Value)
            throw new DomainException("La posición del vecino anterior debe ser menor que la del vecino siguiente.");

        var gap = nextPosition.Value - previousPosition.Value;
        if (gap <= MinimumGap)
        {
            throw new DomainException(
                "No queda espacio suficiente entre las tareas vecinas para reordenar. " +
                "Es necesario rebalancear las posiciones de la columna.");
        }

        return previousPosition.Value + (gap / 2m);
    }
}
