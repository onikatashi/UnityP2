using UnityEngine;


public class PlayerRotate : MonoBehaviour
{
    // 플레이어 좌우 회전처리
    public float speed = 360f;

    // 회전각도 직접 제어하기
    float angleX;

    // Update is called once per frame
    void Update()
    {
        // 회전하기
        Rotate();
    }

    void Rotate()
    {
        if (Input.GetMouseButton(1))
        {
            float h = Input.GetAxis("Mouse X");
            angleX += h * speed * Time.deltaTime;
            transform.eulerAngles = new Vector3(0, angleX, 0);
        }
    }
}
