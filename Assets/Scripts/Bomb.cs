using UnityEngine;

/// <summary>
/// 날아가다 충돌하면 터진다
/// 수류탄은 생성되자마자 스스로 이동하면 당연히 안된다
/// 플레이어가 직접 던져야 한다
/// 수류탄이 다른 오브젝트들과 충돌하면 터지고 자신도 사라져야 한다
/// </summary>
public class Bomb : MonoBehaviour
{
    public GameObject fxFactory;            // 이펙트 프리팹

    // 충돌처리
    private void OnCollisionEnter(Collision collision)
    {
        // 폭발 이펙트 보여주기
        GameObject fx = Instantiate(fxFactory);
        fx.transform.position = transform.position;

        // 다른 오브젝트 삭제
        // 자기자신도 삭제
        Destroy(gameObject);
    }
}
