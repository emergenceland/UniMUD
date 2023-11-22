using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    public ParticleSystem m_ExplosionParticles;       
    public float m_MaxLifeTime = 2f;                  

    void Update() {
        if(transform.position.y < 0f) {
            m_ExplosionParticles.transform.parent = null;
            m_ExplosionParticles.Play();
            Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);
            Destroy(gameObject);
        }
    }
}
