namespace AnimeCharacters.Events
{
    public class DatabaseEvent { }

    public class PageStateManagerEvent { }

    public class SnackbarEvent
    {
        public SnackbarEvent(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
