namespace Common.Command
{
    public interface ICommandHandler<TCommand, TResult>
    {
        Task<CommandResult<TResult>> Execute(TCommand command);
    }
}
