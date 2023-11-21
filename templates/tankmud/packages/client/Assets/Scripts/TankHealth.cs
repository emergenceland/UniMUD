using System;
using System.Collections.Generic;
using UniRx;
using mud;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using ObservableExtensions = UniRx.ObservableExtensions;

public class TankHealth : MonoBehaviour
{
    public float m_StartingHealth = 100f;
    private float m_CurrentHealth;

    public Slider m_Slider;
    public Image m_FillImage;
    public Color m_FullHealthColor = Color.green;
    public Color m_ZeroHealthColor = Color.red;
    public GameObject m_ExplosionPrefab;
    public GameObject shell;
    private ParticleSystem m_ExplosionParticles;
    private bool m_Dead;

    private NetworkManager net;


    private void Awake()
    {
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();
        m_ExplosionParticles.gameObject.SetActive(false);
    }


    private void OnEnable()
    {
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;

        SetHealthUI();
    }
    
    public void SetHealth(float health)
    {
        m_CurrentHealth = health;
        var initialShellPosition = transform.position;
        initialShellPosition.y += 10;
        Instantiate(shell, initialShellPosition, Quaternion.LookRotation(Vector3.down));
        SetHealthUI();
    }

    private void SetHealthUI()
    {
        // Adjust the value and colour of the slider.
        m_Slider.value = m_CurrentHealth;
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
    }

    public void OnDeath()
    {
        m_Dead = true;
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive(true);
        m_ExplosionParticles.Play();
        gameObject.SetActive(false);
    }
}
