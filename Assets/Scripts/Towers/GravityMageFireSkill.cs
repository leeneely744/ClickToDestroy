using System.Collections.Generic;
using UnityEngine;

public class GravityMageFireSkill : TowerSkill
{
    [SerializeField] private float damageMultiplier = 2f;
    [SerializeField] private GameObject firePrefab;

    private readonly List<GameObject> spawnedFires = new List<GameObject>();
    private float worldRadius;

    private void Start()
    {
        var arc = transform.Find("AttackRangeCircle");
        if (arc != null)
        {
            var col = arc.GetComponent<CircleCollider2D>();
            if (col != null)
                worldRadius = col.radius * arc.lossyScale.x;
        }
    }

    protected override void OnActivate()
    {
        var controller = Owner as GravityMageAttackController;
        controller?.SetDamageMultiplier(damageMultiplier);
        SpawnFiresOnRoads();
    }

    private void SpawnFiresOnRoads()
    {
        if (firePrefab == null) return;
        var roadsParent = GameObject.Find("Roads");
        if (roadsParent == null) return;

        foreach (Transform road in roadsParent.transform)
        {
            if (Vector2.Distance(road.position, transform.position) <= worldRadius)
            {
                var fire = Instantiate(firePrefab, road.position, Quaternion.identity);
                spawnedFires.Add(fire);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var fire in spawnedFires)
            if (fire != null) Destroy(fire);
        spawnedFires.Clear();
    }
}
