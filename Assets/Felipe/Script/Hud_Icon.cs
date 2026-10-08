
using UnityEngine;

public class Hud_Icon : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject hook;
    public GameObject boot;

    void Start()
    {
        hook = GameObject.Find("Canvas").transform.Find("Felipe").gameObject.transform.Find("hook").gameObject;
        boot = GameObject.Find("Canvas").transform.Find("Felipe").gameObject.transform.Find("boot").gameObject;
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hook"))
        {
            // Handle hook collision

            hook.SetActive(true);
        }
        if (other.gameObject.CompareTag("Boot"))
        {
            // Handle boot collision
            boot.SetActive(true);

        }
    }
}
