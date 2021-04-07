using System.Collections.Generic;
using UnityEngine;

public class EUnitMovement : MonoBehaviour
{
    [Header("Stats")]
    public EnemyType myType;
    public int health = 50;
    public float attackDelay = 2f;
    public int dmg = 10;
    public float speed = 30f;
    private float rotateSpeed = 2f;
    //private float range = 5f;
    public float horizontalSize = 2f;

    [Header("My do")]
    public EnemyDo myDo = EnemyDo.None;
    public EnemyDo nextDo = EnemyDo.None;
    public Vector2 destinationOfMove;
    public List<Vector2Int> path = new List<Vector2Int>();
    private Vector2Int myDir;
    private Vector2Int myTabPos;
    
    [Header("Other Veribals")]
    //public Vector2Int SpownerPos;
    private float actSpeed;
    private Quaternion direction;
    public float delayToChanePosition = 7f;
    private float avatimeToMoveWhenWait = 0f;
    private Transform tempTargetT;
    public Transform mainTargetT;
    public EnemyBaseControler myEBC;

    private float distanceTraveled = 0;
    private float avaDistToCheckObstacle = 5f;

    private bool attackApproach = true;
    private float avaDillDmgTime = 0;


    void Start()
    {
        if (WorldMenager.instance.loadingWorld == false)
        {
            SetToWait();
        }
    }

    void Update()
    {
        switch (myDo)
        {
            case EnemyDo.Wait: if (WorldMenager.instance.worldTime >= avatimeToMoveWhenWait) { SetToWait(); } break;
            case EnemyDo.Attacking: Attacking(); break;
            case EnemyDo.Moving: Moving(); break;
        }
    }

    
    private void Moving()
    {
        if (destinationOfMove.x == -1) { SetToWait(); }

        transform.rotation = Quaternion.Slerp(transform.rotation, direction, rotateSpeed * Time.deltaTime);
        Vector2 dir = destinationOfMove - (Vector2)transform.position;
        float disThisFrame = actSpeed * Time.deltaTime;
        float disToTarget = dir.magnitude;
        if (disToTarget <= disThisFrame)
        {
            //Debug.Log("dotarlem pos: " + (Vector2)transform.position + " target: " + target + " dist to target: " + disToTarget);
            switch (nextDo)
            {
                case EnemyDo.Wait: myDo = EnemyDo.Wait; break;
                case EnemyDo.Attacking: GetNextPointFromPath(); break;
            }
            return;
        }
        if (nextDo == EnemyDo.Attacking &&  distanceTraveled >= avaDistToCheckObstacle)
        {
            avaDistToCheckObstacle = distanceTraveled + 3f;
            CheckObstacle();
        }
        distanceTraveled += disThisFrame;
        transform.Translate(dir.normalized * disThisFrame, Space.World);
    }
    private void Attacking()
    {
        if (tempTargetT == null)
        {
            //Debug.Log("Missing target");
            if (mainTargetT != null) { SetToAttak(mainTargetT); return; }

            SetToWait();
            GetNewTarget();
            return;
        }

        Vector2 dir = (Vector2)tempTargetT.position - (Vector2)transform.position;
        float disToTarget = dir.magnitude;
        float disThisFrame = actSpeed * Time.deltaTime;
        if (attackApproach)
        {
            if (WorldMenager.instance.worldTime < avaDillDmgTime) { return; }
            if (disToTarget <= 5f+horizontalSize)
            {
                attackApproach = false;
                avaDillDmgTime = WorldMenager.instance.worldTime + attackDelay;
                BulletManager.instance.HitSmt(tempTargetT, dmg);
                return;
            }
            transform.Translate(dir.normalized * disThisFrame, Space.World);
        }
        else
        {
            if (disToTarget >= 10f+horizontalSize)
            {
                attackApproach = true;
                return;
            }
            transform.Translate(-dir.normalized * disThisFrame, Space.World);
        }
    }

    public void SetToWait()
    {
        nextDo = EnemyDo.Wait;
        actSpeed = speed / 2;

        avatimeToMoveWhenWait = WorldMenager.instance.worldTime + Random.Range(delayToChanePosition - 3f, delayToChanePosition + 3f);
        Vector2Int pos = WorldMenager.instance.GetTabPos(transform.position);
        List<Vector2Int> shiftList = new List<Vector2Int>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (WorldMenager.instance.GetSquer(x + pos.x, y + pos.y) == Obj.EnemysTerrain) { shiftList.Add(new Vector2Int(x, y)); }
            }
        }
        if (shiftList.Count > 0)
        {
            Vector2Int shift = shiftList[Random.Range(0, shiftList.Count)];
            //Debug.Log("przesuwam o: " + shift + " z: " + pos);
            float ssh = 3f;
            Vector2 smallShift = new Vector2(Random.Range(-ssh, ssh), Random.Range(-ssh, ssh));
            SetToMove((pos + shift) * 10 + smallShift);
        }
        else
        {
            myDo = EnemyDo.None;
            //Debug.Log("none task.");
        }
    }

    public void SetToMove(Vector2 _target)
    {
        myDo = EnemyDo.Moving;
        destinationOfMove = _target;

        Vector2 dir = _target - (Vector2)transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        direction = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void SetToAttak(Transform _targetT)
    {
        tempTargetT = _targetT;
        actSpeed = speed;
        Vector2 dir = (Vector2)tempTargetT.position - (Vector2)transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        direction = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = direction;
        float disToTarget = dir.magnitude;
        myTabPos = WorldMenager.instance.GetTabPos(transform.position);
        if (disToTarget <= 20f)
        {
            myDo = EnemyDo.Attacking;
            nextDo = EnemyDo.Attacking;
            attackApproach = true;
        }
        else
        {
            myDo = EnemyDo.Moving;
            nextDo = EnemyDo.Attacking;
            if (path.Count == 0) { Debug.Log("Missing path to target"); SetToWait(); }
            else { SetToMove(path[0] * 10); }  
        }
    }
    public void SetToMainAttack(Transform _targetT, List<Vector2Int> _path)
    {
        if (_targetT == null)
        {
            SetToWait();
            GetNewTarget();
            return;
        }

        path = _path;
        mainTargetT = _targetT;
        SetToAttak(_targetT);
        GetNextPointFromPath();
    }
    private void CheckTargetInRange()
    {

    }
    private void CheckObstacle()
    {
        //Debug.Log("Check obstacle");

        if (tempTargetT == null)
        {
            //Debug.Log("Missing target");
            if (mainTargetT != null) { SetToAttak(mainTargetT); return; }

            SetToWait();
            GetNewTarget();
            return;
        }

        Vector2 dir = (Vector2)tempTargetT.position - (Vector2)transform.position;
        float disToTarget = dir.magnitude;
        if (disToTarget <= 12f)
        {
            //Debug.Log("Found main target. Disance: " + disToTarget);
            SetToAttak(tempTargetT);
            return;
        }

        myTabPos = WorldMenager.instance.GetTabPos(transform.position);
        if (path.Count == 0) { Debug.Log("Path is empty"); return; }
        myDir = EnemyControler.instance.GetDir(myTabPos.x, myTabPos.y, path[0].x, path[0].y);
        Vector2Int nextPos = myTabPos + myDir;
        Obj obj = WorldMenager.instance.GetSquer(nextPos.x, nextPos.y);
        if (AllRecipes.instance.IsItBuilding(obj) || AllRecipes.instance.IsItTerrainObj(obj))
        {
            dir = (Vector2)nextPos*10 - (Vector2)transform.position;
            disToTarget = dir.magnitude;
            if (disToTarget <= 12f)
            {
                //Debug.Log("Found obstacle. Distance: "+ disToTarget);
                SetToAttak(WorldMenager.instance.GetTransforOfObj(nextPos.x, nextPos.y));
                return;
            }
        }
    }
    private void GetNextPointFromPath()
    {
        if (path.Count > 0)
        {
            if(path.Count > 1)
            {
                Vector2 dir = (Vector2)path[0] * 10 - (Vector2)transform.position;
                float disToTarget = dir.magnitude;
                if (disToTarget <= 10f)
                {
                    path.RemoveAt(0);
                }
            }

            SetToMove(path[0] * 10);
        }
        else
        {
            Debug.Log("cant get next point from path. path lenght == 0. so try get new target");
            SetToWait();
            GetNewTarget();
        }
        CheckObstacle();
    }
    private void GetNewTarget()
    {
        //Debug.Log("Try get new target");
        myEBC.AddToQueToGetNewTarget(this);
    }

    public void KillUnit()
    {
        myEBC.RemoveUnitFromList(gameObject, myType);
        Destroy(gameObject);
    }
}
