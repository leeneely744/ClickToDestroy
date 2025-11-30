using UnityEngine;

public class GuardianController : MonoBehaviour
{
    private int hp = 100;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);
            GuardianTowerControllerBase guardianTower = GetComponentInParent<GuardianTowerControllerBase>();
            if (guardianTower != null)
            {
                // AttackInterval 秒後にガーディアンを再生成
                guardianTower.OnGuardianDestroyed(guardianTower.AttackInterval);
            }
        }
    }
}
