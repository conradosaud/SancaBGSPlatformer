using UnityEngine;

namespace SancaBGSPlatformer
{
    /// <summary>
    /// Trigger de Início de Speedrun.
    /// Coloque este componente em um Collider (marcado como Is Trigger) na largada do percurso.
    /// </summary>
    public class SpeedrunStartTrigger : MonoBehaviour
    {
        [Tooltip("Referência opcional para o SpeedrunTimer. Se nulo, usará a instância única (SpeedrunTimer.Instance).")]
        public SpeedrunTimer speedrunTimer;

        [Tooltip("Se marcado como true, o gatilho funcionará apenas uma vez por tentativa.")]
        public bool triggerOnlyOnce = false;

        [Header("Efeitos Opcionais")]
        public GameObject startVFX;
        public AudioClip startSFX;

        private bool _hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnlyOnce && _hasTriggered) return;

            // Verifica se o objeto que colidiu é o jogador
            if (other.CompareTag("Player") || other.GetComponentInParent<StarterAssets.ThirdPersonController>() != null || other.GetComponent<CharacterController>() != null)
            {
                SpeedrunTimer timer = speedrunTimer != null ? speedrunTimer : SpeedrunTimer.Instance;

                if (timer != null)
                {
                    timer.StartTimer();
                    _hasTriggered = true;

                    if (startVFX != null)
                    {
                        Instantiate(startVFX, transform.position, Quaternion.identity);
                    }

                    if (startSFX != null)
                    {
                        AudioSource.PlayClipAtPoint(startSFX, transform.position);
                    }
                }
                else
                {
                    Debug.LogWarning("[SpeedrunStartTrigger] Nenhum SpeedrunTimer foi encontrado na cena!");
                }
            }
        }

        public void ResetTrigger()
        {
            _hasTriggered = false;
        }
    }
}
