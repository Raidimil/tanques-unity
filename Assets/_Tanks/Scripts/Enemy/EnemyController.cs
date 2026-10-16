using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float m_Speed = 4f;
    public float m_TurnSpeed = 5f;
    public float m_ContactDamagePerSecond = 10f;
    public float m_SearchInterval = 0.5f;

    private Rigidbody m_Rigidbody;
    private Transform m_Target;
    private float m_NextSearch;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void FindTarget()
    {
        TankHealth[] tanks = FindObjectsOfType<TankHealth>();
        float best = float.MaxValue;
        m_Target = null;

        for (int i = 0; i < tanks.Length; i++)
        {
            float d = (tanks[i].transform.position - transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                m_Target = tanks[i].transform;
            }
        }
    }

    private void FixedUpdate()
    {
        if (Time.time >= m_NextSearch)
        {
            m_NextSearch = Time.time + m_SearchInterval;
            FindTarget();
        }

        if (m_Target == null || !m_Target.gameObject.activeInHierarchy)
            return;

        Vector3 dir = m_Target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
            return;
        dir.Normalize();

        Quaternion look = Quaternion.LookRotation(dir);
        m_Rigidbody.MoveRotation(Quaternion.Slerp(m_Rigidbody.rotation, look, m_TurnSpeed * Time.fixedDeltaTime));
        m_Rigidbody.MovePosition(m_Rigidbody.position + dir * m_Speed * Time.fixedDeltaTime);
    }

    private void OnCollisionStay(Collision collision)
    {
        TankHealth health = collision.collider.GetComponentInParent<TankHealth>();
        if (health != null)
            health.TakeDamage(m_ContactDamagePerSecond * Time.fixedDeltaTime);
    }
}
