namespace TaskQueueLibrary.Enums
{
    // ==========================
    // Перечисление типов команд.
    // ==========================
    public enum ClientCmdType
    {
        SEND_TASK = 0, // Посылка задания.
        GET_STATE = 1, // Запрос состояния.
        GET_RESULT = 2, // Запрос результата.
        FINISH_TASK = 3 // Завершение работы.
    }
}
