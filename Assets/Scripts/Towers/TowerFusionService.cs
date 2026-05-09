using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タワー同士の合成可否判定を行うサービス。
/// TowerController から towerId を取得し、登録された TowerFusionRecipe と突き合わせて
/// 「この2つのタワーを合成すると何になるか」を返す。
/// 実際の GameObject 生成／破棄はここでは行わない。
/// </summary>
public class TowerFusionService
{
    private readonly List<TowerFusionRecipe> recipes = new List<TowerFusionRecipe>();

    public TowerFusionService(IEnumerable<TowerFusionRecipe> initialRecipes = null)
    {
        if (initialRecipes != null)
        {
            recipes.AddRange(initialRecipes);
        }
    }

    /// <summary>
    /// Resources などからレシピをロードして使いたい場合のヘルパー。
    /// 例: new TowerFusionService().LoadFromResources("TowerFusion");
    /// </summary>
    public void LoadFromResources(string folderPath)
    {
        recipes.Clear();
        var loaded = Resources.LoadAll<TowerFusionRecipe>(folderPath);
        if (loaded != null && loaded.Length > 0)
        {
            recipes.AddRange(loaded);
        }
    }

    /// <summary>
    /// 2つのタワーが合成可能かどうかを判定し、成功時は対応するレシピを返す。
    /// </summary>
    public bool CanFuse(TowerController a, TowerController b, out TowerFusionRecipe recipe)
    {
        recipe = null;

        if (a == null || b == null) return false;

        var statsA = a.Stats;
        var statsB = b.Stats;
        if (statsA == null || statsB == null) return false;

        var levelA = GetLevel(statsA, a.levelIndex);
        var levelB = GetLevel(statsB, b.levelIndex);
        if (levelA == null || levelB == null) return false;

        string idA = levelA.towerId;
        string idB = levelB.towerId;
        if (string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB)) return false;

        foreach (var r in recipes)
        {
            if (r != null && r.Matches(idA, idB))
            {
                recipe = r;
                return true;
            }
        }

        return false;
    }

    private TowerLevel GetLevel(TowerStats stats, int index)
    {
        if (stats == null || stats.levels == null || stats.levels.Length == 0)
        {
            return null;
        }

        if (index < 0 || index >= stats.levels.Length)
        {
            return null;
        }

        return stats.levels[index];
    }
}
