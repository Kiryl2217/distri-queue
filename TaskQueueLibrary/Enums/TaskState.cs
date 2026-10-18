namespace TaskQueueLibrary.Enums
{
    // ======================
    // Перечисление состояний
    // задачи
    // ======================
    public enum TaskState
    {
        IN_PROGRESS = 0, // В процессе выполнения.
        COMPLETED = 1, // Выполнено.
        FAILED = 2 // Ошибка.
    }
}
