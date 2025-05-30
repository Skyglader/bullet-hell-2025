using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    private BehaviorTree enemyBehaviorTree;
    private EnemyManager enemyManager;

    public Transform player;
    public float rangeFromPlayer = 2f;
    
    public bool targetInRange = false;

    [Header("Flags")]
    public bool isAlive = true;
    public bool isStunned = false;
    public bool canMove = true;

    // Start is called before the first frame update
    private void Awake()
    {
        enemyManager = GetComponent<EnemyManager>();
    }
    void Start()
    {
        CreateEnemyBehaviorTree();

    }

    // Update is called once per frame
    void Update()
    {
        UpdateRangeToTarget();
        HandleStunnedState();
        enemyBehaviorTree.Process();
    }

    void UpdateRangeToTarget()
    {
        Vector3 adjustedTargetPos = new Vector3(player.position.x, 0f, player.position.z);
        Vector3 adjustedSelfPos = new Vector3(transform.position.x, 0f, transform.position.z);

        targetInRange = Vector3.Distance(adjustedTargetPos, adjustedSelfPos) <= rangeFromPlayer;

    }

    public void ToggleEnemy()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            isAlive = !isAlive;
            GetComponentInChildren<SkinnedMeshRenderer>().enabled = isAlive;
        }

        if (!isAlive)
        {
            Debug.Log("Dead");
        }
    }

    public void CreateEnemyBehaviorTree()
    {
        enemyBehaviorTree = new BehaviorTree("EnemyBehaviorTree");

        Selector activeStatus = new Selector("IsEnemyAlive");
        enemyBehaviorTree.AddChild(activeStatus);

        // Alive
        Sequence isAliveSequence = new Sequence("AliveSequence");
        activeStatus.AddChild(isAliveSequence);
        isAliveSequence.AddChild(new Leaf("CheckIsAlive", new Condition(() => isAlive)));
        isAliveSequence.AddChild(new Leaf("DisableObj", new ActionStrategy(ToggleEnemy)));
        isAliveSequence.AddChild(new Leaf("CheckIsStunned", new Condition(() => !isStunned)));

        // Handles Attack and Movement
        isAliveSequence.AddChild(ActivityTree());


        // Dead
        Sequence isDeadSequence = new Sequence("IsDead");
        activeStatus.AddChild(isDeadSequence);
        isDeadSequence.AddChild(new Leaf("PrintIsDead", new ActionStrategy(ToggleEnemy)));
    }

    public Node ActivityTree()
    {
        Selector rangeSelector = new Selector("RangeSelector");

        // In range
        Sequence inRange = new Sequence("InRange");
        inRange.AddChild(new Leaf("CheckIfInRange", new Condition(() => targetInRange)));
        inRange.AddChild(new Leaf("Attack", new ActionStrategy(HandleAttacks)));

        // Out Range
        Sequence outRange = new Sequence("OutOfRange");
        outRange.AddChild(new Leaf("Move", new ActionStrategy(HandleMovements)));

        rangeSelector.AddChild(inRange);
        rangeSelector.AddChild(outRange);

        return rangeSelector;
    }

    public void HandleAttacks()
    {
    }

    public void HandleMovements()
    {
        if (!enemyManager.agent.enabled)
            return;

        enemyManager.agent.SetDestination(player.position);

        Vector3 velocity = transform.InverseTransformDirection(enemyManager.agent.velocity).normalized;
        enemyManager.enemyAnimationManager.SetMovementParameters(velocity.x, velocity.z);
 
        
    }

    

    public void HandleStunnedState()
    {
        if (isStunned)
        {
            enemyManager.agent.enabled = false;
        }
        else if (canMove) 
        {
            enemyManager.agent.enabled = true;
        }
    }
}
