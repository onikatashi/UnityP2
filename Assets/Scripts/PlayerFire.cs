using UnityEngine;

/// <summary>
/// 1. 총알 발사, 파편튀기 (레이로 충돌처리)
/// 2. 수류탄 발사
/// </summary>
public class PlayerFire : MonoBehaviour
{
    //public GameObject bulletFactory;
    //public GameObject grenadeFactory;
    //public Transform firePoint;
    //public Transform camPoint;

    public Transform firePoint;                     // 총알 발사 위치
    public GameObject bulletImpactFactory;          // 총알 파편 프리팹    
    public GameObject bombFactory;                  // 폭탄 프리팹
    public float throwPower = 10f;                  // 던질 파워

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Fire();
    }

    void Fire()
    {
        // 마우스 왼쪽버튼일 때 레이캐스트로 총알 발사
        if (Input.GetMouseButtonDown(0))
        {
            //GameObject bullet = Instantiate(bulletFactory);
            //bullet.transform.position = firePoint.position;
            //bullet.transform.eulerAngles = camPoint.eulerAngles / 2;

            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;

            //레이랑 충돌했나?
            if (Physics.Raycast(ray, out hit))
            {
                print("충돌 오브젝트: " + hit.collider.name);
                if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    hit.collider.gameObject.GetComponent<EnemyFSM>().HitDamage(10);
                }

                // 충돌지점에 총알 파편만 생성
                GameObject bulletImpact = Instantiate(bulletImpactFactory);

                // 부딪힌 지점
                bulletImpact.transform.position = hit.point;

                // 파편이 부딪힌 지점이 향하는 방향으로 튀게 해줘야 한다
                // Hit 정보안에 노멀벡터의 값도 알 수 있다.
                // 법선벡터 또는 노멀벡터는 평면에 수직인 벡터
                bulletImpact.transform.forward = hit.normal;
            }

            // 레이어 마스크 사용 충돌처리 (최적화)
            // tag보다 약 20배 빠름
            // 총 32비트를 사용하기 때문에 32개까지 추가 가능

            //int layer = gameObject.layer;

            //layer = 1 << 6; // player
            // 0000 0000 0000 0001 => 0000 0000 0010 0000
            // 0000 0000 1000 0000 => Enemy
            // 0000 0000 0000 1000 => Boss
            // 0000 1000 0000 0000 => Player
            // layer = 1 << 8 | 1 << 4 | 1 << 12; => 0000 1000 1000 1000 // 모두 다 충돌처리
            // -> if 문을 한 번에 처리
            // -> 성능 부분에서 최적화
            
            // layer에 ~ 붙이면 해당 레이어를 제외하고 충돌
            //if (Physics.Raycast(ray, out hit, 100, layer))
            //{

            //}

        }

        // 스나이퍼 모드
        if (Input.GetKey(KeyCode.Q))
        {
            Camera.main.fieldOfView = 20f;  // 3배확대
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            Camera.main.fieldOfView = 60f;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            //GameObject grenade = Instantiate(grenadeFactory);
            //grenade.transform.position = firePoint.position;
            //grenade.transform.forward = camPoint.forward * 2 + camPoint.up;

            // 폭탄 생성
            GameObject bomb = Instantiate(bombFactory);
            bomb.transform.position = firePoint.position;

            // 폭탄은 풀레이어가 던지기 때문에
            // 폭탄이 들고 있는 리지드바디를 이용하면 된다
            Rigidbody rb = bomb.GetComponent<Rigidbody>();

            // 전방으로 물리적인 힘을 가한다
            rb.AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);

            // ForceMode.Acceleration   => 연속적인 힘을 가한다 (질량 영향 nope)
            // ForceMode.Force          => 연속적인 힘을 가한다 (질량 영향을 받는다)
            // ForceMode.VelocityChange => 순간적을 힘을 가한다 (질량 영향 nope)
            // ForceMode.Impulse        => 순간적인 힘을 가한다 (질량 영향을 받는다)

            // 45도 정도의 각도로 발사
            // 벡터의 더셈 (Up + Foward)
            // 각도를 낮추고 싶다 => Foward의 길이를 늘려준다
            // 각도를 높이고 싶다 => Up의 길이를 늘려준다
            Vector3 dir = Camera.main.transform.forward + Camera.main.transform.up;
            dir.Normalize();
            rb.AddForce(dir * throwPower, ForceMode.Impulse);
        }

    }
}
