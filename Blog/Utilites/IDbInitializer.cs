using System.Threading.Tasks;

namespace Blog.Utilites
{
    public interface IDbInitializer
    {
        Task Initialize();
    }
}
