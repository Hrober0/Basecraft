using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("To Set")]
    public Transform TurretUp;
    public Transform FirePoint;
    [SerializeField] private string bulletName = "";

    [Header("stats")]
    private BulletsE bulletE;
    public float fireRate = 1f;
    public int dmg = 10;
    public int resistanceBulet = 3;
    public float range = 80f;
    public float turnSpeed = 7f;
    public float needEnergy = 0f;

    [Header("veribal")]
    public float avaShootTime = 0f;
    public int nowResiBulet = 0;

    private TagsEnum targetTag = TagsEnum.Different;
    private bool wasTarget = false;
    public int rangeInt;
    private Vector2Int myPos;

    private bool bulletIsLoaded = false;
    private LineRenderer LineR;
    public ElectricityUser eleUSc;

    private PlatformBehavior PlatformB;
    private Transform TargetT;
    public Res bulletType = Res.None;
    public Res[] posibleAmmo;
    private float avaAmmoUpdateTime = 0f;

    void Awake()
    {
        rangeInt = (int)(range / 10) - 1;
        bulletE = (BulletsE)System.Enum.Parse(typeof(BulletsE), bulletName);
        nowResiBulet = 0;

        PlatformB = GetComponent<PlatformBehavior>();
        PlatformB.usingGuiType = PlatfotmGUIType.Turret;
        PlatformB.itemSendingType = PlatformItemSendingType.Procesing;
        PlatformB.canBeConnectedOut = false;
        PlatformB.SetAllCanInItem(false);
        PlatformB.range = rangeInt;

        switch (bulletE)
        {
            case BulletsE.Quarrel:
                posibleAmmo = new Res[2];
                posibleAmmo[1] = Res.Quarrel;
                posibleAmmo[0] = Res.Quarrel2;
                break;
            case BulletsE.GunB:
                posibleAmmo = new Res[2];
                posibleAmmo[1] = Res.GunMagazine;
                posibleAmmo[0] = Res.GunMagazine2;
                break;
            case BulletsE.Laser:
                break;
            case BulletsE.Rocket:
                posibleAmmo = new Res[1];
                posibleAmmo[0] = Res.Rocket;
                break;
        }

        if (ClickMenager.instance.StartAmmoInTuret)
        {
            if (posibleAmmo != null && posibleAmmo.Length > 0) { PlatformB.itemOnPlatform[(int)posibleAmmo[0]].qua = 100; }
        }

        TurretUp.localRotation = Quaternion.AngleAxis(90f, Vector3.forward);

        if(TryGetComponent(out ElectricityUser tEleUSc))
        {
            eleUSc = tEleUSc;
            eleUSc.maxEnergyPerSec = needEnergy;
            eleUSc.actCharge = 0f;
            eleUSc.maxCharge = eleUSc.maxEnergyPerSec * 2f;
            ElectricityManager.instance.AddRequester(eleUSc);
        }
    }
    void Start()
    {
        myPos = new Vector2Int((int)(transform.position.x / 10), (int)(transform.position.y / 10));

        if (WorldMenager.instance.loadingWorld == false)
        {
            avaShootTime = WorldMenager.instance.worldTime + 1f / fireRate;
        }

        InvokeRepeating("TryFindEnemyBase", 0.1f, 5f);
        InvokeRepeating("UpdateTarget", 0.1f, 0.5f);

        if (bulletE == BulletsE.Laser) { LineR = TurretUp.GetComponent<LineRenderer>(); LineR.enabled = false; }
        if (bulletE == BulletsE.Rocket) { FirePoint.Find("Rocket").gameObject.SetActive(false); InvokeRepeating("TryLoadRocket", 0.1f, 2f); }
    }
    void Update()
    {
        if (TargetT == null)
        {
            if (bulletE == BulletsE.Laser) { if (LineR.enabled == true) { eleUSc.actEnergyPerSec = 0f; LineR.enabled = false; } }
            if (wasTarget) { TryFindEnemyBase(); }
            return; 
        }

        //rotete
        Vector2 dir = TargetT.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotate = Quaternion.AngleAxis(angle, Vector3.forward);
        TurretUp.rotation = Quaternion.Slerp(TurretUp.rotation, rotate, turnSpeed * Time.deltaTime);

        //check rote
        float nr = TurretUp.transform.rotation.eulerAngles.z;
        angle = rotate.eulerAngles.z;
        float dif = (angle - nr)%360;
        if (dif < 0) { dif *= -1; }
        if (dif > 10) 
        {
            if (bulletE == BulletsE.Laser) { if (LineR.enabled == true) { eleUSc.actEnergyPerSec = 0f; LineR.enabled = false; } }
            return;
        }

        //laser
        if (bulletE == BulletsE.Laser)
        {
            if (eleUSc.actCharge < needEnergy * Time.deltaTime) { if (LineR.enabled == true) { eleUSc.actEnergyPerSec = 0f; LineR.enabled = false; } return; }

            if (LineR.enabled == false) { LineR.enabled = true; }

            eleUSc.actCharge -= needEnergy * Time.deltaTime;
            eleUSc.actEnergyPerSec = needEnergy;

            float distance = Vector2.Distance(transform.position, TargetT.position);
            LineR.SetPosition(1, new Vector2(distance, 0));
            if (avaShootTime <= WorldMenager.instance.worldTime)
            {
                Shoot();
                avaShootTime = WorldMenager.instance.worldTime + 1f / fireRate;
            }
            return;
        }

        //shot
        if (nowResiBulet > 0 || bulletIsLoaded)
        {
            if (avaShootTime <= WorldMenager.instance.worldTime)
            {
                Shoot();
                nowResiBulet--;
                fixResi();
                avaShootTime = WorldMenager.instance.worldTime + 1f/fireRate;
            }
        }
        else
        {
            fixResi();
        }

        void fixResi()
        {
            if (bulletType != Res.None && PlatformB.itemOnPlatform[(int)bulletType].qua > 0)
            {
                if (nowResiBulet <= 0)
                {
                    nowResiBulet = resistanceBulet;
                    PlatformB.AddItem(bulletType, -1);
                }
            }
            else
            {
                UpdateAmmo();
            }
        }
    }

    private void UpdateTarget()
    {
        //fix bullet resistanc
        if (bulletE != BulletsE.Laser)
        {
            if (avaAmmoUpdateTime > WorldMenager.instance.worldTime) { return; }
            if (bulletType == Res.None)
            {
                UpdateAmmo();
            }
            else
            {
                if (nowResiBulet <= 0 && PlatformB.itemOnPlatform[(int)bulletType].qua > 0)
                {
                    nowResiBulet = resistanceBulet;
                    PlatformB.AddItem(bulletType, -1);
                }
            }
        }

        if (TargetT != null) { return; }

        //enemy unity
        //Debug.Log("TODO: find neares enemy unit");
        Transform targ = EnemyControler.instance.GetNearesEUnit(transform.position, range);
        if (targ == null) 
        { TargetT = null; }
        else
        { SetTarget(targ, TagsEnum.EnemyUnit); }    
    }
    public void SetTarget(Transform tra, TagsEnum tag)
    {
        TargetT = tra;
        targetTag = tag;
        wasTarget = true;
    }
    private void TryFindEnemyBase()
    {
        if (TargetT != null) { return; }

        Vector2Int nBP = EnemyControler.instance.GetNearestEnemyBuilding(myPos.x, myPos.y);
        if (nBP.x == -1) { wasTarget = false; return; }

        int rl = rangeInt * rangeInt;
        int distance = (myPos.x - nBP.x) * (myPos.x - nBP.x) + (myPos.y - nBP.y) * (myPos.y - nBP.y);
        //Debug.Log("mp pos: " + myPos + " nearest enemy building: " + nBP + " distance: " + distance + " rl: " + rl);
        if(distance > rl) { wasTarget = false; return; }

        Transform foundBaseT = WorldMenager.instance.GetTransforOfObj(nBP.x, nBP.y); ;
        if (foundBaseT == null) { wasTarget = false; Debug.Log("this building has no transform"); return; }

        SetTarget(foundBaseT, TagsEnum.EnemyBase);
    }

    private void Shoot()
    {
        if (bulletE == BulletsE.Laser) { BulletManager.instance.HitSmt(TargetT, dmg); }

        BulletManager.instance.SpownBullet(FirePoint.position, FirePoint.rotation.eulerAngles.z, TargetT, bulletE);
        if (bulletE == BulletsE.Rocket) { FirePoint.Find("Rocket").gameObject.SetActive(false); bulletIsLoaded = false; }
    }
    private void TryLoadRocket()
    {
        if(bulletIsLoaded == true) { return; }

        if (nowResiBulet > 0)
        {
            FirePoint.Find("Rocket").gameObject.SetActive(true);
            bulletIsLoaded = true;
        }
    }

    private void UpdateAmmo()
    {
        if (avaAmmoUpdateTime > WorldMenager.instance.worldTime){ return; }

        avaAmmoUpdateTime = WorldMenager.instance.worldTime + WorldMenager.instance.frequencyOfChecking;

        for (int i = 0; i < posibleAmmo.Length; i++)
        {
            PlatformB.itemOnPlatform[(int)posibleAmmo[i]].canIn = false;
        }

        for (int i = 0; i < posibleAmmo.Length; i++)
        {
            int resI = (int)posibleAmmo[i];
            if(PlatformB.itemOnPlatform[resI].qua > 0)
            {
                bulletType = posibleAmmo[i];
                PlatformB.itemOnPlatform[resI].maxQua = 10;
                PlatformB.itemOnPlatform[resI].canIn = true;
                PlatformB.itemOnPlatform[resI].canOut = false;
                switch (bulletType)
                {
                    case Res.Quarrel: bulletE = BulletsE.Quarrel; break;
                    case Res.Quarrel2: bulletE = BulletsE.Quarrel2; break;
                    case Res.GunMagazine: bulletE = BulletsE.GunB; break;
                    case Res.GunMagazine2: bulletE = BulletsE.GunB2; break;
                    case Res.Rocket: bulletE = BulletsE.Rocket; break;
                }
                dmg = BulletManager.instance.GetDmgOfBullet(bulletE);
                return;
            }
        }

        for (int i = 0; i < posibleAmmo.Length; i++)
        {
            int resI = (int)posibleAmmo[i];
            PlatformB.itemOnPlatform[resI].maxQua = 10;
            PlatformB.itemOnPlatform[resI].canIn = true;
            PlatformB.itemOnPlatform[resI].canOut = false;
        }
    }
}
