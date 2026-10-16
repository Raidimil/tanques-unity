using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float m_StartingHealth = 60f;
    public GameObject m_ExplosionPrefab;

    private float m_CurrentHealth;
    private bool m_Dead;

    private void Awake()
    {
        m_CurrentHealth = m_StartingHealth;
    }

    public void TakeDamage(float amount)
    {
        if (m_Dead)
            return;

        m_CurrentHealth -= amount;
        if (m_CurrentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        m_Dead = true;

        if (m_ExplosionPrefab != null)
        {
            GameObject fx = Instantiate(m_ExplosionPrefab, transform.position, Quaternion.identity);
            fx.SetActive(true);

            ParticleSystem ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(fx, ps.main.duration + 1f);
            }
            else
            {
                Destroy(fx, 3f);
            }

            AudioSource audio = fx.GetComponent<AudioSource>();
            if (audio != null)
                audio.Play();
        }

        Destroy(gameObject);
    }
}