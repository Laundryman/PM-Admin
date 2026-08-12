namespace PM_AdminApp.Server.Exceptions
{
    public class DuplicatePartException : Exception
    {
        public DuplicatePartException()
        {
        }

        public DuplicatePartException(string message)
            : base(message)
        {
        }

        public DuplicatePartException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}