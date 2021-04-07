using UnityEngine;

public class EUnitFlying : MonoBehaviour
{
    [Header("Stats")]
    public EnemyType myType;
    public int health = 50;
    public float attackDelay = 2f;
    public int dmg = 10;
    public float speed = 30f;
    private float rotateSpeed = 2f;
    public float horizontalSize = 2f;

    [Header("My do")]
    public EnemyDo myDo = EnemyDo.None;
    public EnemyDo nextDo = EnemyDo.None;
    public Vector2 targetPos;
    public Transform targetT;

    [Header("Other Veribals")]
    public Vector2 spawnerPos;
    private float distanceRounding = 10f;
    private Quaternion direction;
    private float rotateF;
    private float rotateToChange;
    private float lastDistanceToTarget;
    private float roundingMultiplier;
    private bool waitingState = false;
    public EnemyBaseControler myEBC;

    private bool attackApproach = true;
    private float avaDillDmgTime = 0f;


    void Start()
    {
        if (WorldMenager.instance.loadingWorld == false)
        {
            spawnerPos = transform.position;
            SetToWait();
        }
    }

    void Update()
    {
        switch (myDo)
        {
            case EnemyDo.Wait: Waiting(); break;
            case EnemyDo.Moving: Moving(); break;
            case EnemyDo.Attacking: Attacking(); break;
        }
    }

    private void Attacking()
    {
        if (targetT == null)
        {
            //Debug.Log("Missing target");
            Transform targ = myEBC.GetNearestPlatform(WorldMenager.instance.GetTabPos(transform.position));
            if (targ == null)
            { SetToWait(); }
            else
            { SetToAttack(targ); }
            
            return;
        }

        Vector2 dir = (Vector2)targetT.position - (Vector2)transform.position;
        float disToTarget = dir.magnitude;
        float disThisFrame = speed * Time.deltaTime;
        if (attackApproach)
        {
            if (WorldMenager.instance.worldTime < avaDillDmgTime) { return; }
            if (disToTarget <= 5f+horizontalSize)
            {
                attackApproach = false;
                avaDillDmgTime = WorldMenager.instance.worldTime + attackDelay;
                BulletManager.instance.HitSmt(targetT, dmg);
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
    private void Waiting()
    {
        if (waitingState)
        {
            //circulation
            if (rotateF >= rotateToChange)
            {
                waitingState = false;
                roundingMultiplier /= 3;
                lastDistanceToTarget = (targetPos - (Vector2)transform.position).magnitude;
            }
        }
        else
        {
            //passing
            float disToTarget = (targetPos - (Vector2)transform.position).magnitude;
            if (disToTarget > lastDistanceToTarget && disToTarget > distanceRounding)
            {
                waitingState = true;
                if (rotateF >= 360) { rotateF = rotateF % 360; }
                rotateToChange = rotateF + 270f;
                roundingMultiplier = Random.Range(0.4f, 0.7f);
            }
            lastDistanceToTarget = disToTarget;
        }
        
        rotateF += speed * distanceRounding * roundingMultiplier * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, 0, rotateF);
        transform.Translate(speed * Time.deltaTime, 0, 0);
    }
    private void Moving()
    {
        if (targetT == null)
        {
            Transform targ = myEBC.GetNearestPlatform(WorldMenager.instance.GetTabPos(transform.position));
            if (targ == null)
            { SetToWait(); }
            else
            { SetToAttack(targ); }
            return;
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, direction, rotateSpeed * Time.deltaTime);
        Vector2 dir = targetPos - (Vector2)transform.position;
        float disThisFrame = speed * Time.deltaTime;
        float disToTarget = dir.magnitude - horizontalSize - 5f;
        if (disToTarget <= disThisFrame)
        {
            switch (nextDo)
            {
                case EnemyDo.Attacking: SetToAttack(targetT); return;
            }
            return;
        }
       
        transform.Translate(dir.normalized * disThisFrame, Space.World);
    }

    public void SetToWait()
    {
        rotateF = transform.rotation.eulerAngles.z;
        rotateToChange = rotateF + 270f;
        roundingMultiplier = 0.54f;
        targetPos = spawnerPos;
        myDo = EnemyDo.Wait;
        waitingState = true;
    }

    public void SetToMoving(Vector2 _targetPos)
    {
        myDo = EnemyDo.Moving;

        targetPos = _targetPos;

        Vector2 dir = targetPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        direction = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SetToAttack(Transform target)
    {
        if (target == null)
        {
            Transform targ = myEBC.GetNearestPlatform(WorldMenager.instance.GetTabPos(transform.position));
            if (targ == null)
            { SetToWait(); }
            else
            { SetToAttack(targ); }
            return;
        }
        targetT = target;
        Vector2 dir = (Vector2)targetT.position - (Vector2)transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        direction = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = direction;
        float disToTarget = dir.magnitude;

        if (disToTarget <= 10f+horizontalSize)
        {
            myDo = EnemyDo.Attacking;
            nextDo = EnemyDo.Attacking;
            attackApproach = true;
        }
        else
        {
            myDo = EnemyDo.Moving;
            nextDo = EnemyDo.Attacking;
            SetToMoving(target.position);
        }
    }

    public void KillUnit()
    {
        myEBC.RemoveUnitFromList(gameObject, myType);
        Destroy(gameObject);
    }
}
