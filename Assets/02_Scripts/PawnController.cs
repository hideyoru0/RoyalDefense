using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PawnController : MonoBehaviour
{
    public enum LIVINGENTITYSTATE
    {
        None = -1,
        IDLE = 0,
        WALK,
        ATTACK,
        HIT,
        DIE
    }
    public LIVINGENTITYSTATE pawnState;

    public string pawnName;
    private AttackController attackController;
    private NavMeshAgent navAgent;
    private Animator animator;
    private Collider _collider;
    public Slider hpSlider;

    public Transform moveTarget;
    public Transform attackTarget;

    public GameObject damageEffect;
    public GameObject attackEffect;
    public GameObject attackRangeEffect;
    public GameObject levelUpEffect;

    public GameObject clickObject;

    public int level = 1;
    public int health;
    public int maxHealth;
    public int createPrice;
    public int upgradePrice;

    public float moveSpeed;
    public float rotateSpeed;
    public int power;

    public float attackDistance;
    public float attackCoolTime;
    public float currentTime;
    public float hitTime;

    public bool isDead = false;
    public bool isAttack = false;
    public bool isHit = false;

    void Start()
    {
        attackController = GetComponent<AttackController>();
        navAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
        animator = GetComponentInChildren<Animator>();

        health = maxHealth;
    }

    void Update()
    {
        StateCheck();
        TargetCheck();
    }

    public void StateCheck()
    {
        if (isDead)
            return;

        currentTime += Time.deltaTime;

        if (!GameManager.Instance().isNight)
        {
            if(attackEffect != null)
            {
                attackEffect.SetActive(false);
                attackRangeEffect.SetActive(false);
            }
            pawnState = LIVINGENTITYSTATE.IDLE;
            Destroy(clickObject);
        }

        switch (pawnState)
        {
            case LIVINGENTITYSTATE.None:
                break;
            case LIVINGENTITYSTATE.IDLE:
                animator.SetInteger("LIVINGENTITYSTATE", (int)pawnState);
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero;

                break;
            case LIVINGENTITYSTATE.WALK:
                animator.SetInteger("LIVINGENTITYSTATE", (int)pawnState);
                navAgent.isStopped = false;
                navAgent.speed = moveSpeed;

                float distance = Vector3.Distance(transform.position, moveTarget.position);

                if (distance <= 1.0f)
                {
                    pawnState = LIVINGENTITYSTATE.IDLE;
                    Destroy(clickObject);
                }
                else
                {
                    navAgent.SetDestination(moveTarget.position);
                }

                break;
            case LIVINGENTITYSTATE.ATTACK:
                if (currentTime > attackCoolTime)
                {
                    animator.SetInteger("LIVINGENTITYSTATE", (int)pawnState);
                    currentTime = 0;

                    if (attackController.attackType == AttackController.ATTACKTYPE.SINGLE)
                    {
                        if (pawnName.Equals("Knight"))
                        {
                            AudioManager.Instance().SoundPlay(AudioManager.Instance().pawnAttackSound[0]);
                        }
                        else
                        {
                            AudioManager.Instance().SoundPlay(AudioManager.Instance().pawnAttackSound[1]);
                        }

                        attackController.SingleAttack();
                        pawnState = LIVINGENTITYSTATE.IDLE;
                    }
                    else
                    {
                        AudioManager.Instance().SoundPlay(AudioManager.Instance().pawnAttackSound[2]);

                        pawnState = LIVINGENTITYSTATE.IDLE;
                        StartCoroutine(WizardAttack());
                        attackController.RangeAttack();
                    }
                }
                else if(attackController.attackType != AttackController.ATTACKTYPE.SINGLE)
                {
                    animator.SetInteger("LIVINGENTITYSTATE", 0);
                    navAgent.isStopped = true;
                    navAgent.velocity = Vector3.zero;
                }

                if (attackTarget == null)
                {
                    pawnState = LIVINGENTITYSTATE.IDLE;
                    return;
                }

                distance = Vector3.Distance(transform.position, attackTarget.position);

                if (distance > attackDistance)
                {
                    pawnState = LIVINGENTITYSTATE.IDLE;
                }

                break;
            case LIVINGENTITYSTATE.DIE:
                StartCoroutine(OnDie());

                break;
            default:
                break;
        }
    }

    public void TargetCheck()
    {
        if (isDead)
            return;

        if(pawnState == LIVINGENTITYSTATE.WALK)
        {
            return;
        }

        currentTime += Time.deltaTime;
        Collider[] _coll = Physics.OverlapSphere(transform.position, 4.0f, 1 << 6);

        if (_coll.Length > 0)
        {
            float minDis = Vector3.Distance(transform.position, _coll[0].transform.position);
            int minIdx = 0;

            for (int i = 0; i < _coll.Length; i++)
            {
                if (minDis >= Vector3.Distance(transform.position, _coll[i].transform.position))
                {
                    minDis = Vector3.Distance(transform.position, _coll[i].transform.position);
                    minIdx = i;
                }
            }

            attackTarget = _coll[minIdx].transform;
            transform.LookAt(attackTarget);
            pawnState = LIVINGENTITYSTATE.ATTACK;
        }
    }

    public void OnDamage(int _power)
    {
        StartCoroutine(DamageEffect());
        health -= _power;
        hpSlider.value = (float)health / maxHealth;

        if (health <= 0)
        {
            AudioManager.Instance().SoundPlay(AudioManager.Instance().pawnDieSound);
            Destroy(clickObject);
            pawnState = LIVINGENTITYSTATE.DIE;
        }
    }

    public IEnumerator DamageEffect()
    {
        damageEffect.SetActive(true);
        yield return new WaitForSeconds(0.35f);
        damageEffect.SetActive(false);
    }

    IEnumerator OnDie()
    {
        animator.SetTrigger("DIE");
        _collider.enabled = false;
        navAgent.isStopped = true;
        navAgent.velocity = Vector3.zero;
        isDead = true;

        yield return new WaitForSeconds(1.0f);

        Destroy(gameObject);
    }

    IEnumerator WizardAttack()
    {
        attackEffect.SetActive(true);
        attackRangeEffect.SetActive(true);

        yield return new WaitForSeconds(4.0f);

        attackEffect.SetActive(false);
        attackRangeEffect.SetActive(false);
    }

    public void LevelUP()
    {
        StartCoroutine(LevelUPEffect());

        level += 1;
        maxHealth += 50;
        power += 5;
        attackCoolTime -= 0.5f;
        moveSpeed -= 0.2f;
        attackDistance += 3.0f;

        health = maxHealth;
        upgradePrice += 2;

        hpSlider.value = (float)health / maxHealth;
    }

    public IEnumerator LevelUPEffect()
    {
        levelUpEffect.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        levelUpEffect.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
