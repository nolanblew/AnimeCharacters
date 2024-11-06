using System.Threading.Tasks;

namespace AnimeCharacters.Shared
{
    public interface IUpdateAvailableDetector
    {
        Task CheckForUpdate();
    }
}
