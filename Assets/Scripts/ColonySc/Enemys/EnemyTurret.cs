using UnityEngine;

public class EnemyTurret : MonoBehaviour
{
    [Header("To Set")]
    public Transform TurretUp;
    public Transform FirePoint;

    [Header("stats")]
    [SerializeField] float fireRate = 1f;
    [SerializeField] float turnSpeed = 7f;

    [Header("veribal")]
    private Transform TargetT = null;
    public float avaShootTime;
    private Vector2Int myPos;
    //private bool wasTarget = false;

    void Start()
    {
        myPos = new Vector2Int((int)(transform.position.x / 10), (int)(transform.position.y / 10));
        InvokeRepeating("UpdateTarget", 0.1f, 1f);
    }

    void Update()
    {
        if (TargetT == null) { return; }

        Vector2 dir = TargetT.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotate = Quaternion.AngleAxis(angle, Vector3.forward);
        TurretUp.rotation = Quaternion.Slerp(TurretUp.rotation, rotate, turnSpeed * Time.deltaTime);

        if (avaShootTime <= WorldMenager.instance.worldTime)
        {
            Shoot();
            avaShootTime = WorldMenager.instance.worldTime + 1f/fireRate;
        }
    }

    void UpdateTarget()
    {
        Vector2Int foundTarget = EnemyControler.instance.GetNearestBuilding(myPos);
        if (foundTarget.x == -1){ return; }
        
        TargetT = WorldMenager.instance.GetTransforOfObj(foundTarget.x, foundTarget.y);
    }
    void Shoot()
    {
        BulletManager.instance.SpownBullet(FirePoint.position, FirePoint.rotation.eulerAngles.z, TargetT, BulletsE.EneTurB);
    }
}
