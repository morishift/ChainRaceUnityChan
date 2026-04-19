using UnityEngine;

namespace Sample
{
    /// <summary>
    /// AnimatorのIdle状態に入るべきかどうかを判定する
    /// Speed と Direction の両方が一定時間閾値未満なら ShouldIdle を true にする
    /// </summary>
    public class ShouldIdle : MonoBehaviour
    {
        [SerializeField]
        Animator animator;
        [SerializeField]
        float requiredDuration = 0.25f;
        [SerializeField]
        float speedThreshold = 0.1f;
        [SerializeField]
        float directionThreshold = 0.1f;

        float lowSpeedTimer = 0f;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// AnimatorのSpeedとDirectionを監視し、両方が閾値未満の状態が一定時間続いたらShouldIdleをtrueにし、Idle状態へ遷移させる。
        /// </summary>
        void Update()
        {
            float speed = animator.GetFloat("Speed");
            float direction = animator.GetFloat("Direction");

            if (speed < speedThreshold && Mathf.Abs(direction) < directionThreshold)
            {
                lowSpeedTimer += Time.deltaTime;
                if (lowSpeedTimer >= requiredDuration)
                {
                    //Debug.Log($"lowSpeedTimer:{lowSpeedTimer} ShouldIdle:{true} speed:{speed} direction:{direction}");
                    animator.SetBool("ShouldIdle", true);
                }
            }
            else
            {
                lowSpeedTimer = 0f;
                //Debug.Log($"lowSpeedTimer:{lowSpeedTimer} ShouldIdle:{false} speed:{speed} direction:{direction}");
                animator.SetBool("ShouldIdle", false);
            }
        }
    }
}
