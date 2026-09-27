using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.iOS; // Namespace obrigatório para o novo Input System

public class CheckPoint : MonoBehaviour
{
    public GameObject flag;

    void Start()
    {
        flag = GameObject.FindGameObjectWithTag("Flag");
    }

    void Update()
    {
        // Verifica se o teclado está conectado e se a tecla F foi pressionada neste frame
        if (transform.position.y <= -7)
        {
            if(flag != null)
            {
                transform.position = new Vector3(flag.transform.position.x, flag.transform.position.y, flag.transform.position.z);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Check"))
        {
            // Handle collision with the checkpoint
            transform.position = new Vector3(flag.transform.position.x, flag.transform.position.y, flag.transform.position.z);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Check"))
        {
            // Handle collision with the checkpoint
            transform.position = new Vector3(flag.transform.position.x, flag.transform.position.y, flag.transform.position.z);
        }
    }

}
