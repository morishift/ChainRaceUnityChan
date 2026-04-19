using UnityEngine;

namespace Sample
{
    public class CameraDirection : MonoBehaviour
    {
        [SerializeField] Transform targetFace;
        [SerializeField] Transform targetHead;
        [SerializeField] Transform targetChest;
        [SerializeField] Transform targetFoot;

        [SerializeField]
        [Range(0f, 1f)]
        float approachRatePerSecond = 0.1f; // 1秒間に残り角度を何割縮めるか

        [SerializeField]
        public bool targetFacePriority = true; // trueなら顔を優先、falseなら胸を優先

        Camera cam;

        void Awake()
        {
            cam = GetComponent<Camera>();
        }

        void LateUpdate()
        {
            if (targetChest == null) return;

            Vector3 aimDir = CalcAimDirection();
            Quaternion targetRot = Quaternion.LookRotation(aimDir);
            float t = 1f - Mathf.Pow(1f - approachRatePerSecond, Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t);
        }

        public void ResetDirection()
        {
            Vector3 aimDir = CalcAimDirection();
            transform.rotation = Quaternion.LookRotation(aimDir);
        }

        Vector3 CalcAimDirection()
        {
            float fov = cam != null ? cam.fieldOfView : 60f;

            // Case 0: 顔
            if (targetFacePriority && targetFace != null)
            {
                return (targetFace.position - transform.position).normalized;
            }

            // Case 1: 頭〜足が収まる → 角二等分線方向（上端=head, 下端=foot）
            if (targetHead != null && targetFoot != null && FitsInView(targetHead.position, targetFoot.position))
            {
                Vector3 toHead = (targetHead.position - transform.position).normalized;
                Vector3 toFoot = (targetFoot.position - transform.position).normalized;
                return (toHead + toFoot).normalized;
            }

            // Case 2: 頭〜胸が収まる → headがちょうど上端になる方向
            if (targetHead != null && FitsInView(targetHead.position, targetChest.position))
            {
                Vector3 toHead = (targetHead.position - transform.position).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, toHead).normalized;
                // headの方向からFOV/2だけ下に回転した方向がカメラ中心 → headが上端に一致
                return Quaternion.AngleAxis(fov * 0.5f, right) * toHead;
            }

            // Case 3: 胸に向ける
            return (targetChest.position - transform.position).normalized;
        }

        bool FitsInView(Vector3 a, Vector3 b)
        {
            float fov = cam != null ? cam.fieldOfView : 60f;
            Vector3 toA = a - transform.position;
            Vector3 toB = b - transform.position;
            return Vector3.Angle(toA, toB) <= fov;
        }
    }
}
