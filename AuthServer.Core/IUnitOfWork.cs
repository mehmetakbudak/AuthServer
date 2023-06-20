using System.Threading.Tasks;

namespace AuthServer.Core
{
    public interface IUnitOfWork
    {
        Task CommitAsync();
        void Commit();
    }
}
