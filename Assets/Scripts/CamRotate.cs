using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 카메라를 마우스 움직이는 방향으로 회전하기
/// </summary>
public class CamRotate : MonoBehaviour
{

    public float speed = 360f;          // 회전 속도 (Time.deltaTime 곱해서 1초당 200도 회전)
    float angleX, angleY;               // 직접 제어할 회전 각도

    // Update is called once per frame
    void Update()
    {
        // 카메라 회전
        Rotate();
    }

    void Rotate()
    {
        if (Input.GetMouseButton(1))
        {
            float h = Input.GetAxis("Mouse X");         // 마우스 가로 움직임
            float v = Input.GetAxis("Mouse Y");         // 마우스 세로 움직임

            //Vector3 dir = new Vector3(-v, h, 0);        // 회전 방향 벡터

            // 회전은 각각의 축을 기준으로 회전을 한다
            //transform.Rotate(dir * speed * Time.deltaTime);

            // 유니티 엔진 내부적으로 -각도는 360도를 더한 값으로 변환해서 처리한다
            // 따라서 우리가 직접 각도를 제어해서 사용해야 회전 처리가 편하다
            angleX += h * speed * Time.deltaTime;       // 가로 움직임으로 X축 회전 각도 변경
            angleY += v * speed * Time.deltaTime;       // 세로 움직임으로 Y축 회전 각도 변경
            angleY = Mathf.Clamp(angleY, -70f, 70f);    // Y축 회전 각도 제한
            transform.rotation = Quaternion.Euler(-angleY, angleX, 0);      // 회전 각도 설정
        }
    }
}
