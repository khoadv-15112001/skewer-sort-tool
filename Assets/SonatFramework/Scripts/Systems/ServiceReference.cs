using System;
using Sirenix.OdinInspector;

namespace SonatFramework.Systems
{
    [Serializable]
    public class Service<T> where T : class
    {
        [EnumToggleButtons] public ReferenceType referenceType = ReferenceType.Automatic;

        [ShowIf("@referenceType == ReferenceType.Manual")]
        public T reference;

        private static T instance;
        public T Instance => referenceType == ReferenceType.Manual
            ? reference
            : instance ??= SonatSystem.GetService<T>();

        public static T Get()
        {
            return instance ??= SonatSystem.GetService<T>();
        }
        
        public Service()
        {
            
        }

        public Service(ReferenceType referenceType)
        {
            this.referenceType = referenceType;
        }
    }

    public enum ReferenceType
    {
        Manual,
        Automatic
    }
}