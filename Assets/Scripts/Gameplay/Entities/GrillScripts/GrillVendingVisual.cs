using System.Collections.Generic;
using DG.Tweening;
using Gameplay.LevelData;
using Manager;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using TMPro;
using UnityEngine;

namespace Gameplay.Entities.GrillScripts
{
    public class GrillVendingVisual : GrillVisual
    {
        [SerializeField] private TMP_Text txtNumLayer;
        [SerializeField] private Transform container;
        [SerializeField] private Transform progress;

        private List<VendingGrillTileInProgress> tiles = new();
        private readonly Service<PoolingService> poolingService = new();

        public bool isSingle = true;

        public override void SetDefaultGrill(GrillData grillData)
        {
            base.SetDefaultGrill(grillData);

            int numLayer = grillData.layer.Count;
            txtNumLayer.text = numLayer.ToString();
            progress.gameObject.SetActive(true);
            tiles.Clear();
            for (int i = 0; i < numLayer; i++)
            {
                var tile = poolingService.Instance.Create<VendingGrillTileInProgress>("VendingGrillTileInProgress");

                tile.Setup(i, numLayer, container);
                tiles.Add(tile);
            }
        }

        protected override void SetVisual()
        {
            if (isSingle)
            {
                SetSingleVisual();
                return;
            }

            SetNormalVisual();

        }

        private void SetNormalVisual()
        {
            if (GameplayController.instance.levelGenerator.LevelData.levelType is LevelType.Cake or LevelType.Fruit)
            {
                stove.SetSpriteAsync(PathManager.TraySprite("Fruit"));
                lid.SetSpriteAsync(PathManager.LidSprite("Fruit"));
                return;
            }

            stove.SetSpriteAsync(PathManager.TraySprite("Normal"));
            lid.SetSpriteAsync(PathManager.LidSprite("Normal"));
        }

        private void SetSingleVisual()
        {
            if (GameplayController.instance.levelGenerator.LevelData.levelType is LevelType.Cake or LevelType.Fruit)
            {
                stove.SetSpriteAsync(PathManager.TraySprite("Fruit_Single"));
                lid.SetSpriteAsync(PathManager.LidSprite("Fruit_Single"));
                return;
            }

            stove.SetSpriteAsync(PathManager.TraySprite("Normal_Single"));
            lid.SetSpriteAsync(PathManager.LidSprite("Normal_Single"));
        }

        // public override void UpdateSubGrill()
        // {
        //     if( numLayer <= 0) return;
        //     numLayer--;
        //     txtNumLayer.text = numLayer.ToString();
        //
        //     tiles[numLayer].transform.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
        //     {
        //         poolingService.Instance.ReturnObj(tiles[numLayer]);
        //         tiles.RemoveAt(numLayer);
        //     });
        //
        //     if (numLayer == 0)
        //     {
        //         primaryVendingGrill.SetLockItems(true);
        //         SonatUtils.DelayCall(0.5f, CloseGrill, this);
        //     }
        // }

        public void UpdateLayer(int numLayer)
        {
            txtNumLayer.text = numLayer.ToString();

            tiles[numLayer].transform.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                poolingService.Instance.ReturnObj(tiles[numLayer]);
                tiles.RemoveAt(numLayer);
            });
        }

        public void SetLayer(int numLayer)
        {
            txtNumLayer.text = numLayer.ToString();

            for (int i = 0; i < tiles.Count; i++)
            {
                if (i >= numLayer)
                    tiles[i].transform.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
                    {
                        poolingService.Instance.ReturnObj(tiles[numLayer]);
                        tiles.RemoveAt(numLayer);
                    });
            }
        }

        public override void OnReturnGrill()
        {
            base.OnReturnGrill();
            if (tiles != null)
            {
                foreach (var tile in tiles)
                {
                    poolingService.Instance.ReturnObj(tile);
                }
            }

            tiles.Clear();
        }

        public void Unlock()
        {
            progress.gameObject.SetActive(false);
        }
    }
}