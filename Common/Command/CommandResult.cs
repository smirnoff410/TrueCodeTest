namespace Common.Command
{
    public class CommandResult<T>
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Response { get; set; }
    }
}
