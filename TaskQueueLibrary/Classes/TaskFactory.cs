using TaskQueueLibrary.Enums;

namespace TaskQueueLibrary.Classes
{
    public class TaskFactory
    {
        public static UserTask Create(byte id, byte command, byte type, byte subscription, byte[] values)
        {
            byte priority;

            switch ((TaskType)type)
            {
                case TaskType.FACTORIAL:
                case TaskType.FIBONACCI:
                    priority = 1;
                    break;
                case TaskType.COUNT_WORDS:
                case TaskType.HASH:
                    priority = 2;
                    break;
                case TaskType.SORT:
                case TaskType.SUM:
                    priority = 3;
                    break;

                default:
                    throw new Exception("Не реализовано.");
            }

            return new UserTask(id, command, (TaskType)type, subscription, priority, values);
        }


    }
}