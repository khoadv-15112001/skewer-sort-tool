namespace SonatFramework.Systems.ConfigManagement
{
    public abstract class ConfigService: SonatServiceSo
    {
        public abstract T Get<T>() where T : class;
    }
}