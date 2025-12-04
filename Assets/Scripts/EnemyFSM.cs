using System.Collections;
using UnityEngine;
using UnityEngine.AI;   // 네비메시에이전트 사용하려면 반드시 필요함

public class EnemyFSM : MonoBehaviour
{
    /*
     * 유한 상태 머신 (FSM)
     * => 유한한 수의 상태(state)와 상태들 사이의 전환(transition)을 정의해서
     * 시스템 동작을 하게 하는것을 말하고, 당연히 전환이 이루어 질려면 조건(condition)이 필요하다.
     * 이것은 FSM 디자인 패턴이다
     * - 상태: 간단하게 행동들 (걷기, 달리기, 점프, 공격, 죽음 등등)
     * - 전환: 상태에서 상태로 넘어가는 변화
     * - 조건: 전환이 발생하기 위한 필요한 기준 (키 입력, HP 감소 특정 아이템을 획득 등 다양한 이벤트)
     * => 결론은 이미 여러분은 애니메이터를 사용하면서 한번 씩은 겪어 봤다.
     * 
     * => 플레이어 캐릭터 행동 제어
     * => 대표적으로 에너미 AI 구현
     * => 예) 몬스터가 플레이어를 발견하기 전에는 (순찰) 상태, 플레이어를 발견하면 (추격) 상태,
     * 공격 범위 안에 들어오면 (공격) 상태, HP가 일정이하로 떨어졌을 때 (도망, 버서커)
     */

    // 몬스터 상태 이넘문
    enum EnemyState
    {
        Idle, Move, Attack, Return, Damaged, Die
    }

    EnemyState preState;        // 몬스터 이전 상태
    EnemyState state;           // 몬스터 상태 변수

    public float findRange = 15f;       // 플레이어를 찾는 범위
    public float moveRange = 30f;       // 시작지점에서 최대 이동 가능한 범위
    public float attackRange = 5f;      // 공격 가능 범위

    // 애니메이션을 제어하기 위한 애니메이션 컴포넌트
    //Animator anim;

    Transform playerPos;         // 플레이어 위치 (코드로 처리)
    Vector3 firstPos;                   // 몬스터 처음 위치

    CharacterController cc;             // 캐릭터 컨트롤러 컴포넌트
    float speed = 0.7f;                 // 이동속도

    float attackCooldown = 1f;          // 공격 쿨타임
    float timer = 1f;                   // 공격 시간 쿨타임 재기

    float hp = 50f;                     // 몬스터 체력
    bool isDamaged = false;             // 대미지 확인
    bool isDie = false;                 // 죽음 확인

    // 유니티에서 길찾기 알고리즘이 적용이된 네비게이션을 사용하려면 반드시 UnityEngine.Ai 추가해야 함
    // 네비게이션은 맵 전체를 베이크 해서 에이전트가 어느 위치에 있던 미리 계산된 정보를 사용한다.
    NavMeshAgent agent;
    // 에이전트 사용 시 주의사항
    // 충돌은 콜리더로 하고
    // 이동만 네비메시에이전트를 사용해야
    // EnemyFSM을 제대로 사용할 수 있다.
    // 충돌이 제대로 작동안할 수도 있다
    // 따라서 시작할 때 네비메시에이전트는 꺼줘야 한다.


    Animator anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
        playerPos = GameObject.Find("Player").transform;

        anim = GetComponent<Animator>();

        // 몬스터 상태 초기화
        state = EnemyState.Idle;
        preState = state;

        firstPos = transform.position;

        agent = GetComponent<NavMeshAgent>();
        Debug.Log(agent);
        agent.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 상태에 따른 행동처리
        switch (state)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Move:
                Move();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Return:
                Return();
                break;
            case EnemyState.Damaged:
                Damaged();
                break;
            case EnemyState.Die:
                Die();
                break;
        }
    }

    // 대기 상태
    void Idle()
    {
        if (!isDie)
        {
            agent.enabled = false;
            // 1. 플레이어와 일정범위가 되면 이동상태로 변경 (탐지범위)
            // - 플레이어 찾기
            // - 일정거리 비교 (Distance, magnitude, sqrMagnitude 아무거나)
            // - 상태변경 state = EnemyState.Move
            // - 상태전환 출력 print("Idel -> Move")
            // - 애니메이션 anim.SetTrigger("Move")

            anim.SetBool("IsMove", false);

            if (Vector3.Distance(playerPos.position, transform.position) < findRange)
            {
                preState = state;
                state = EnemyState.Move;
                print("Idle -> Move");
            }
        }
    }
    
    // 이동 상태
    void Move()
    {
        if (!isDie)
        {
            anim.SetBool("IsMove", true);

            // 시작할 때 켜주고 무브 상태가 아닐 때 꺼줘야 한다.
            if (!agent.enabled) agent.enabled = true;

            Vector3 dir = playerPos.position - transform.position;

            // 몬스터가 자신이 서 있는 위치에서 회전 값 없이 백스텝으로 쫓아온다.
            // 몬스터가 타겟을 바로 보도록 하자
            //transform.forward = dir;
            //transform.LookAt(dir);

            // 좀 더 자연스럽게
            //transform.forward = Vector3.Lerp(transform.forward, dir, 5 * Time.deltaTime);
            // 여기에 문제가 한가지 있는데
            // 타겟과 몬스터가 일직선 상일 경우 왼쪽으로 회전해야 할지 오른쪽으로 회전해야 할지 정하질 못해서
            // 그냥 백덤블링으로 회전을 하게 한다.
            // -> 쿼터니온으로 회전처리 하면 좀 더 자연스럽다

            // 최종적으로 자연스런 회전처리를 하려면 결국 쿼터니온을 사용해야 한다
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(dir), 4 * Time.deltaTime);

            cc.SimpleMove(dir * speed);

            // 플레이어를 향해서 이동해라
            // 네비메시에이전트가 회전처리부터 이동까지 전부 다 처리해준다
            //agent.SetDestination(playerPos.position);

            // 플레이어를 향해서 이동해러
            // 네비메시에이전트가 회전처리부터 이동까지 전부 다 처리해준다.
            // 네비메시에이전트 선언 해준 후 밑에꺼 (destination)
            //NavMeshAgent.SetDestination(playerPos.position);


            // 1. 플레이어를 향해 이동 후 공격범위 안에 들어오면 공격 상태로 변경
            if (Vector3.Distance(playerPos.position, transform.position) < attackRange)
            {
                preState = state;
                state = EnemyState.Attack;
                print("Move -> Attack");
            }
            // 2. 플레이어를 추격하더라도 처음 위치에서 일정 범위를 넘어가면 리턴 상태로 변경

            if (Vector3.Distance(transform.position, firstPos) > moveRange)
            {
                preState = state;
                state = EnemyState.Return;
                print("Move -> Return");
            }

            if (Vector3.Distance(transform.position, playerPos.position) > findRange)
            {
                preState = state;
                state = EnemyState.Return;
                print("Move -> Return");
            }
            // - 플레이어처럼 캐릭터 컨트롤러 이용하기 (cc.Move 대신 cc.SimpleMove 이용하자)
            // - 공격범위 2미터
            // - 상태변경
            // - 상태전환 출력
            // - 애니메이션
        }
    }

    // 공격 상태
    void Attack()
    {
        if (!isDie)
        {
            anim.SetBool("IsMove", false);

            // 에이전트 오프
            agent.enabled = false;

            // 공격할 때 거리로만 처리되다보니 엉뚱한 곳을 공격할 수 있다
            transform.LookAt(playerPos.position);

            // 1. 플레이어가 공격범위 안에 있다면 일정 시간 간격으로 플레이어 공격
            timer += Time.deltaTime;
            if (timer >= attackCooldown)
            {
                anim.SetTrigger("IsAttack");
                print("몬스터 공격");
                timer = 0;
            }
            // 2. 플레이어가 공격범위를 벗어났다면 이동상태(재추격)로 변경
            if (Vector3.Distance(playerPos.position, transform.position) > attackRange)
            {
                preState = state;
                state = EnemyState.Move;
                print("Attack -> Move");
                timer = attackCooldown;
            }
            // - 공격 범위 2미터
            // - 상태변경
            // - 상태전환 출력
            // - 애니메이션
        }
    }

    // 복귀 상태
    void Return()
    {
        if (!isDie)
        {
            if (!agent.enabled) agent.enabled = true;

            anim.SetBool("IsMove", true);
            // 1. 몬스터가 플레이어를 추격하더라도 처음 위치에서 일정 범위를 벗어나면 다시 돌아옴
            Vector3 dir = firstPos - transform.position;

            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(dir), 4 * Time.deltaTime);

            cc.SimpleMove(dir * speed);

            // 이동 처리
            //agent.SetDestination(firstPos);

            if (Vector3.Distance(transform.position, firstPos) < 1f)
            {
                transform.position = firstPos;
                preState = state;
                state = EnemyState.Idle;
                print("Return -> Idle");
                agent.enabled = false;
            }
            if (Vector3.Distance(transform.position, playerPos.position) < findRange
                && Vector3.Distance(playerPos.position, firstPos) < moveRange)
            {
                preState = state;
                state = EnemyState.Move;
                print("Return -> Move");
            }
            // - 처음 위치에서 일정범위 30미터
            // - 상태변경
            // - 상태전환 출력
            // - 애니메이션
        }
    }

    // 플레이어 쪽에서 충돌감지를 할 수 있으니 이 함수는 퍼블릭으로 만들자
    public void HitDamage(int value)
    {
        
        // 예외처리
        // 피격 상태거나, 죽음 상태일 때는 데미지 중첩으로 주지 않는다.
        if(state == EnemyState.Damaged || state == EnemyState.Die)
        {
            print("이미 피격 중 또는 사망");
            return;
        }

        hp -= value;
        if( hp > 0)
        {
            preState = state;
            state = EnemyState.Damaged;
            print(preState + " ->  Damaged");
        }
        if( hp <= 0)
        {
            preState = state;
            state = EnemyState.Die;
            print(preState + " -> Die");
        }
        // 체력 깎기
        // 몬스터 체력이 1 이상이면 피격 상태
        // 0 이하면 죽음 상태
    }

    // 피격 상태 (Any State)
    void Damaged()
    {
        if (!isDie)
        {
            agent.enabled = false;
            // 1. 몬스터 체력이 1이상
            // 2. 다시 이전상태로 변경
            // - 상태변경
            // - 상태전환 출력
            StartCoroutine(DamagedCoroutine());
            // 피격상태를 처리하기 위해서는 간단한 코루틴 사용
        }
    }

    IEnumerator DamagedCoroutine()
    {
        if (!isDamaged)
        {
            anim.SetTrigger("IsDamaged");
            isDamaged = true;
            print("피격 애니메이션");
            print("몬스터 체력: " + hp);
            yield return new WaitForSeconds(1);
            state = preState;
            preState = EnemyState.Damaged;
            print("Damaged -> " + preState);
            isDamaged = false;
        }
    }

    // 죽음 상태 (Any State)
    void Die()
    {
        agent.enabled = false;
        // 1. 몬스터 체력 0이하
        // 2. 몬스터 오브젝트 삭제
        // - 상태변경
        // - 상태전환 출력

        // 진행중인 모든 코루틴은 정지 한다
        // StopAllCoroutines();
        // 죽음 상태를 처리하기 위해서는 간단한 코루틴 사용하자.
        if (!isDie)
        {
            StopAllCoroutines();
            StartCoroutine(DieCoroutine());
        }
    }

    IEnumerator DieCoroutine()
    {
        isDie = true;
        print("죽는 애니메이션");
        anim.SetTrigger("IsDie");
        yield return new WaitForSeconds(1f);
        print(preState + " -> Die");
        Destroy(gameObject);
    }


    private void OnDrawGizmos()
    {
        // 공격 가능 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 플레이어 찾을 수 있는 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, findRange);

        // 이동 가능한 최대 범위
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(firstPos, moveRange);
    }
}
