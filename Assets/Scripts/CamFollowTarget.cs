using UnityEngine;

public class CamFollowTarget : MonoBehaviour
{
    [SerializeField] Transform target;

    void Update()
    {
        // 미니맵 카메라가 플레이어 따라다니기
        FollowTarget();
    }

    void FollowTarget()
    {
        if (target == null) return;
        // 타겟(플레이어)이 이동하는 X, Z값만 있으면 된다.
        // 높이값은 본인 자신의 값 그대로 사용하자
        transform.position = new Vector3(
            target.position.x,
            transform.position.y,
            target.position.z);
    }
}
