using Cysharp.Threading.Tasks;

namespace SonatFramework.Systems
{
    public interface IServiceInitializeAsync
    {
        public UniTaskVoid InitializeAsync();
    }
}