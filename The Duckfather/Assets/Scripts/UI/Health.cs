using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int health;
    
    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    
    [PunRPC]
    public void TakeDamage(int _damage)
    {
        health -= _damage;
        
        healthSlider.value = health;
        
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
