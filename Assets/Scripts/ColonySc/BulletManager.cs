using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager instance;
    

    public GameObject Quarrel;
    public GameObject Quarrel2;
    public GameObject GunB;
    public GameObject GunB2;
    public GameObject Rocket;

    public GameObject EneTurB;

    private float bulletUpdateRange = 40f;

    void Awake()
    {
        if (instance != null) { Debug.Log("more the one BulletManager on scen"); return; }
        instance = this;

        //bulletUpdateRangeInt = (int)(bulletUpdateRange / 10f) - 1;
    }

    public void SpownBullet(Vector2 pos, float rotate, Transform TargetT, BulletsE bulletE)
    {
        if (TargetT == null) { Debug.Log("mising TT"); return; }

        if (bulletE == BulletsE.Laser) { return; }

        GameObject GO = BulletEToGO(bulletE);
        if (GO == null) { Debug.Log("ERROE! mising GameObject!"); return; }

        GameObject newBulet = Instantiate(GO, pos, Quaternion.Euler(0, 0, rotate));
        BulletBehavior BuletB = newBulet.GetComponent<BulletBehavior>();
        if (BuletB == null) { Debug.Log("mising BB"); return; }
        BuletB.Set(TargetT, bulletE);
    }
    public int GetDmgOfBullet(BulletsE bulletE)
    {
        GameObject bGO = BulletEToGO(bulletE);
        if (bGO == null) { return -1; }
        BulletBehavior bSc = bGO.GetComponent<BulletBehavior>();
        if (bSc == null) { return -1; }
        return bSc.dmg;
    }
    public void HitSmt(Transform targetT, int dmg)
    {
        if (targetT == null) { return; }
        
        Vector2Int pos = new Vector2Int((int)(targetT.position.x / 10), (int)(targetT.position.y / 10));
        TagsEnum tagE = WorldMenager.instance.TagToTagEnum(targetT.tag);
        if (tagE == TagsEnum.Different) { Debug.Log("ERROR! Wrong tag"); return; }
        switch (tagE)
        {
            case TagsEnum.EnemyUnit:
                EUnitMovement eMSc = targetT.GetComponent<EUnitMovement>(); if (eMSc != null) { eMSc.health -= dmg; if (eMSc.health <= 0) { eMSc.KillUnit(); } break; }
                EUnitFlying eFSc = targetT.GetComponent<EUnitFlying>();     if (eFSc != null) { eFSc.health -= dmg; if (eFSc.health <= 0) { eFSc.KillUnit(); } break; }
                break;
            case TagsEnum.EnemyBase:
                Square square = WorldGrid.GetSquare(pos.x, pos.y);
                if (square == null) { Debug.Log("ERROR! Object on " + pos + " doesn't exist in WorldGrid"); break; }
                square.health -= dmg;
                if (square.health <= 0)
                {
                    WorldMenager.instance.RemoveObjFromPos(pos.x, pos.y);
                }
                break;
            case TagsEnum.Platform:
                square = WorldGrid.GetSquare(pos.x, pos.y);
                if (square == null) { Debug.Log("ERROR! Object on " + pos + " doesn't exist in WorldGrid"); break; }
                square.health -= dmg;
                if (square.health <= 0)
                {
                    PlatformBehavior PBSc = targetT.GetComponent<PlatformBehavior>();
                    PBSc.RemovePlatform();
                }
                break;
            case TagsEnum.ObjectPlan:
                BuildMenager.instance.RemoveBuildingPlan(pos.x, pos.y);
                break;
            case TagsEnum.Wall:
                square = WorldGrid.GetSquare(pos.x, pos.y);
                if (square == null) { Debug.Log("ERROR! Object on " + pos + " doesn't exist in WorldGrid"); break; }
                square.health -= dmg;
                if (square.health <= 0)
                {
                    WorldMenager.instance.RemoveObjFromGO(targetT.gameObject, pos.x, pos.y);
                }
                break;
            case TagsEnum.TerrainObj:
                square = WorldGrid.GetSquare(pos.x, pos.y);
                if (square == null) { Debug.Log("ERROR! Object on " + pos + " doesn't exist in WorldGrid"); break; }
                square.health -= dmg;
                if (square.health <= 0)
                {
                    WorldMenager.instance.RemoveObjFromGO(targetT.gameObject, pos.x, pos.y);
                }
                break;
        }
    }
    private GameObject BulletEToGO(BulletsE bE)
    {
        switch (bE)
        {
            case BulletsE.Quarrel: return Quarrel;
            case BulletsE.Quarrel2: return Quarrel2;
            case BulletsE.GunB: return GunB;
            case BulletsE.GunB2: return GunB2;
            case BulletsE.Rocket: return Rocket;

            case BulletsE.EneTurB: return EneTurB;
        }
        return null;
    }
    public Transform GetTransNearTarget(Vector2 bullPos, TagsEnum tagE)
    {
        //float shortestDistance = Mathf.Infinity;
        //int targetIndex = -1;
        Transform target = null;
        Vector2Int posInt = WorldMenager.instance.GetTabPos(bullPos);
        float rl = bulletUpdateRange * bulletUpdateRange;

        if (tagE == TagsEnum.EnemyUnit || tagE == TagsEnum.EnemyBase)
        {
            //try find enemy base
            Vector2Int tPos = EnemyControler.instance.GetNearestEnemyBuilding(posInt.x, posInt.y);
            if (tPos.x != -1) { target = WorldMenager.instance.GetTransforOfObj(tPos.x, tPos.y); }
            if (target != null)
            {
                if (CalcDist(bullPos, target.position) <= rl) { return target; }
            }

            //try find enemy
            target = EnemyControler.instance.GetNearesEUnit(bullPos, bulletUpdateRange);
            if (target != null)
            {
                if (CalcDist(bullPos, target.position) <= rl) { return target; }
            }
        }
        else if(tagE == TagsEnum.Platform || tagE == TagsEnum.Wall || tagE == TagsEnum.Electricity)
        {
            Debug.Log("TODO: bullet target update when target is missing");
        }
        

        return null;
    }
    private float CalcDist(Vector2 sPos, Vector2 ePos)
    {
        float dx = ePos.x - sPos.x;
        float dy = ePos.y - sPos.y;
        return dx * dx + dy * dy;
    }
}
