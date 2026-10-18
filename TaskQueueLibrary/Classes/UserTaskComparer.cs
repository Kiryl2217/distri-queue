namespace TaskQueueLibrary.Classes
{
    // ========================
    // Сортировка компаратором.
    // ========================
    public class UserTaskComparer : IComparer<UserTask>
    {

        // От большего приоритета к меньшему.
        public int Compare(UserTask x, UserTask y)
            => y.priority.CompareTo(x.priority);
    }
}
