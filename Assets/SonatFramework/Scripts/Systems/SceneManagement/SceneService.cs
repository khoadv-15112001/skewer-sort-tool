using System;
using Sonat.Enums;
using UnityEngine.SceneManagement;

namespace SonatFramework.Systems.SceneManagement
{
    public abstract class SceneService : SonatServiceSo
    {
        public abstract GamePlacement GetCurrentGamePlacement();
        public abstract void SwitchScene(GamePlacement newPlacement, bool force = false, Action callback = null);
    }
}