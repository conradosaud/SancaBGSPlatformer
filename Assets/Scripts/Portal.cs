using System.Collections;
using UnityEngine;
using StarterAssets;

namespace SancaBGSPlatformer
{
    /// <summary>
    /// Sistema de Portal com efeito de viagem suave entre Portal A e Portal B.
    /// Desliga a malha do personagem, ativa o efeito visual de viagem e move o jogador até o destino.
    /// </summary>
    public class Portal : MonoBehaviour
    {
        [Header("Conexão do Portal")]
        [Tooltip("Portal de destino ou ponto de saída para onde o jogador será transportado.")]
        public Transform exitPoint;

        [Tooltip("Portal de destino (opcional). Se definido, ativa o cooldown no portal de destino para evitar loops.")]
        public Portal targetPortal;

        [Header("Configurações Visuais")]
        [Tooltip("Geometria/Malha do personagem que será DESLIGADA durante o teleporte.")]
        public GameObject characterVisual;

        [Tooltip("Objeto/Efeito de viagem que será LIGADO para acompanhar o deslocamento do jogador.")]
        public GameObject travelVisual;

        [Header("Configurações de Movimento")]
        [Tooltip("Velocidade de navegação/teleporte do jogador até a saída do portal.")]
        public float travelSpeed = 25.0f;

        [Tooltip("Tempo de espera (cooldown) antes que este portal possa ser reutilizado.")]
        public float portalCooldown = 1.0f;

        [Header("Estado de Execução")]
        public bool isTeleporting = false;

        private float _cooldownTimer = 0.0f;

        private void Awake()
        {
            // Garante que o efeito visual de viagem comece desativado
            if (travelVisual != null)
            {
                travelVisual.SetActive(false);
            }
        }

        private void Update()
        {
            if (_cooldownTimer > 0)
            {
                _cooldownTimer -= Time.deltaTime;
            }
        }

        public void TriggerCooldown()
        {
            _cooldownTimer = portalCooldown;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isTeleporting || _cooldownTimer > 0) return;

            // Verifica se o objeto que entrou é o jogador
            if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null || other.GetComponent<ThirdPersonController>() != null)
            {
                Transform targetDestination = GetDestination();
                if (targetDestination != null)
                {
                    StartCoroutine(TeleportRoutine(other.gameObject, targetDestination));
                }
                else
                {
                    Debug.LogWarning($"[Portal] {gameObject.name} não possui um exitPoint ou targetPortal configurado!");
                }
            }
        }

        private Transform GetDestination()
        {
            if (exitPoint != null) return exitPoint;
            if (targetPortal != null)
            {
                return targetPortal.exitPoint != null ? targetPortal.exitPoint : targetPortal.transform;
            }
            return null;
        }

        private IEnumerator TeleportRoutine(GameObject player, Transform destination)
        {

            player.GetComponentInParent<ThirdPersonController>().isTeleporting = true;

            isTeleporting = true;
            TriggerCooldown();

            if (targetPortal != null)
            {
                targetPortal.TriggerCooldown();
            }

            // Cancela habilidades ativas e zera velocidades na entrada do portal
            ResetPlayerState(player);

            // Tenta obter componentes do jogador para desativar física e movimentação durante o efeito
            CharacterController characterController = player.GetComponent<CharacterController>();
            ThirdPersonController thirdPersonController = player.GetComponent<ThirdPersonController>();

            if (characterController != null) characterController.enabled = false;
            if (thirdPersonController != null) thirdPersonController.enabled = false;

            // Tenta encontrar a geometria do personagem se não tiver sido arrastada manualmente
            GameObject visualToDisable = characterVisual;
            if (visualToDisable == null)
            {
                // Busca um filho comum de modelo (como Geometry ou PlayerArmature)
                Transform geomTransform = player.transform.Find("Geometry") ?? player.transform.Find("PlayerArmature");
                if (geomTransform != null)
                {
                    visualToDisable = geomTransform.gameObject;
                }
            }

            // 1. Desliga a malha do personagem
            if (visualToDisable != null)
            {
                visualToDisable.SetActive(false);
            }

            // 2. Liga o objeto visual de viagem
            if (travelVisual != null)
            {
                travelVisual.SetActive(true);
            }

            Vector3 startPosition = player.transform.position;
            Vector3 targetPosition = destination.position;
            Quaternion targetRotation = destination.rotation;

            // Posiciona o objeto visual de viagem na posição inicial
            if (travelVisual != null)
            {
                travelVisual.transform.position = startPosition;
            }

            float totalDistance = Vector3.Distance(startPosition, targetPosition);
            
            if (totalDistance > 0.01f)
            {
                while (Vector3.Distance(player.transform.position, targetPosition) > 0.1f)
                {
                    // Move o objeto do jogador em direção ao destino
                    player.transform.position = Vector3.MoveTowards(
                        player.transform.position,
                        targetPosition,
                        travelSpeed * Time.deltaTime
                    );

                    // Atualiza a posição do objeto de viagem para acompanhar o movimento
                    if (travelVisual != null)
                    {
                        travelVisual.transform.position = player.transform.position;
                        if (player.transform.position != targetPosition)
                        {
                            travelVisual.transform.rotation = Quaternion.LookRotation(targetPosition - player.transform.position);
                        }
                    }

                    yield return null;
                }
            }

            // Garante posição e rotação exatas na saída
            player.transform.position = targetPosition;
            player.transform.rotation = targetRotation;

            // 3. Desliga o objeto de viagem ao chegar na saída
            if (travelVisual != null)
            {
                travelVisual.transform.position = targetPosition;
                travelVisual.SetActive(false);
            }

            // 4. Religa a malha do personagem
            if (visualToDisable != null)
            {
                visualToDisable.SetActive(true);
            }

            // Restaura controles e física
            if (characterController != null) characterController.enabled = true;
            if (thirdPersonController != null) thirdPersonController.enabled = true;

            // Cancela acúmulo de inércia e reseta o estado do jogador na saída do portal
            ResetPlayerState(player);

            isTeleporting = false;
            player.GetComponentInParent<ThirdPersonController>().isTeleporting = false;
        }

        /// <summary>
        /// Reseta a velocidade vertical e horizontal do jogador, cancela poderes/upgrades ativos (Grapple, SuperJump, Glide)
        /// e limpa os comandos de entrada para evitar arremessos ou reações indesejadas na saída do portal.
        /// </summary>
        private void ResetPlayerState(GameObject player)
        {
            // Reseta a velocidade do ThirdPersonController (evita ser arremessado ao sair)
            ThirdPersonController thirdPerson = player.GetComponent<ThirdPersonController>();
            if (thirdPerson != null)
            {
                thirdPerson.ResetVerticalVelocity();
                thirdPerson.isMovementLocked = false;
                thirdPerson.customGravityScale = 1.0f;
            }

            // Reseta velocidade de Rigidbody (caso o jogador possua)
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector3.zero;
#else
                rb.velocity = Vector3.zero;
#endif
                rb.angularVelocity = Vector3.zero;
            }

            // Desativa/Cancela os poderes e upgrades ativos
            GrappleHook grapple = player.GetComponent<GrappleHook>();
            if (grapple != null)
            {
                grapple.StopGrapple();
            }

            SuperJump superJump = player.GetComponent<SuperJump>();
            if (superJump != null)
            {
                superJump.CancelCharge();
                superJump.CancelSuperJump();
            }

            GlideAbility glide = player.GetComponent<GlideAbility>();
            if (glide != null)
            {
                glide.StopGliding();
            }

            // Reseta estados de input
            StarterAssetsInputs inputs = player.GetComponent<StarterAssetsInputs>();
            if (inputs != null)
            {
                inputs.jump = false;
                inputs.sprint = false;
                inputs.grapple = false;
                inputs.superJump = false;
                inputs.move = Vector2.zero;
            }
        }

        private void OnDrawGizmos()
        {
            // Desenha uma linha no Editor conectando o portal ao seu destino para fácil visualização
            Transform dest = GetDestination();
            if (dest != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, dest.position);
                Gizmos.DrawWireSphere(dest.position, 0.5f);
            }
        }
    }
}
