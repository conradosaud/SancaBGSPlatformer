using UnityEngine;
using UnityEngine.InputSystem; // Namespace obrigatório para o novo Input System

public class Flag : MonoBehaviour
{
    public GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        // Verifica se o teclado está conectado e se a tecla F foi pressionada neste frame
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (player != null)
            {
                transform.position = new Vector3(player.transform.position.x +1, player.transform.position.y+1, player.transform.position.z);
            }
        }
    }
}
