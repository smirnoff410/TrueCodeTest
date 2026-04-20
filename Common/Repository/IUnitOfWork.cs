namespace Common.Repository
{
    public interface IUnitOfWork
    {
        Task Save();
    }
}
