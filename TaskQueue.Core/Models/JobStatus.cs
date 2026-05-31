namespace TaskQueue.Core.Models;

public enum JobStatus
{
    Pending = 0,   // czeka w kolejce
    Processing = 1,  // worker je przetwarza
    Completed = 2,   // zakończone sukcesem
    Failed = 3,   // nie udało się
    Retrying = 4    // czeka na ponowną próbę
}