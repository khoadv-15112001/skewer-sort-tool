using Cysharp.Threading.Tasks;

namespace UI.Elements
{
    public interface IHomeProcess
    {
        public UniTask<bool> ProcessTask();
    }
}