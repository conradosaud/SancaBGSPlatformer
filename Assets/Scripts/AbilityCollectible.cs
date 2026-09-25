using System.Collections;
using UnityEngine;
using TMPro;

namespace SancaBGSPlatformer
{
    /// <summary>
    /// Coletável (Orbe) para desbloquear habilidades do jogador (Gancho, Super Pulo e/ou Planar).
    /// Suporta exibição de texto TextMeshProUGUI com fade de transição via CanvasGroup.
    /// </summary>
    public class AbilityCollectible : MonoBehaviour
    {
        [Header("Habilidades a Desbloquear")]
        [Tooltip("Se marcado, desbloqueia a habilidade de Gancho (Grapple Hook).")]
        public bool unlockGrapple = false;

        [Tooltip("Se marcado, desbloqueia a habilidade de Super Pulo (Super Jump).")]
        public bool unlockSuperJump = false;

        [Tooltip("Se marcado, desbloqueia a habilidade de Planar (Glide).")]
        public bool unlockGlide = false;

        [Header("Interface UI (TextMeshPro & CanvasGroup)")]
        [Tooltip("Componente TextMeshProUGUI que exibirá o texto do upgrade.")]
        public TextMeshProUGUI unlockText;

        [Tooltip("CanvasGroup principal associado ao texto para controlar o fade de opacidade.")]
        public CanvasGroup textCanvasGroup;

        [Header("Mensagens Customizadas (Opcional)")]
        public string grappleUnlockMessage = "GANCHO DESBLOQUEADO!";
        public string superJumpUnlockMessage = "SUPER PULO DESBLOQUEADO!";
        public string glideUnlockMessage = "PLANAR DESBLOQUEADO!";

        [Header("CanvasGroups Específicos por Habilidade (Opcional)")]
        [Tooltip("Se quiser associar um CanvasGroup específico para o Gancho.")]
        public CanvasGroup grappleCanvasGroup;

        [Tooltip("Se quiser associar um CanvasGroup específico para o Super Pulo.")]
        public CanvasGroup superJumpCanvasGroup;

        [Tooltip("Se quiser associar um CanvasGroup específico para o Planar.")]
        public CanvasGroup glideCanvasGroup;

        [Header("Efeitos Visuais & Sonoros (Opcional)")]
        public GameObject pickupVFX;
        public AudioClip pickupSFX;

        [Header("Animação do Coletável")]
        public bool rotateVisual = true;
        public float rotationSpeed = 90.0f;
        public bool bobVisual = true;
        public float bobFrequency = 2.0f;
        public float bobAmplitude = 0.25f;

        private Vector3 _startPosition;
        private bool _isCollected = false;

        private void Start()
        {
            _startPosition = transform.position;

            // Esconde os CanvasGroups no início se estiverem definidos
            if (textCanvasGroup != null) textCanvasGroup.alpha = 0f;
            if (grappleCanvasGroup != null) grappleCanvasGroup.alpha = 0f;
            if (superJumpCanvasGroup != null) superJumpCanvasGroup.alpha = 0f;
            if (glideCanvasGroup != null) glideCanvasGroup.alpha = 0f;
        }

        private void Update()
        {
            if (_isCollected) return;

            // Animação visual no mundo 3D
            if (rotateVisual)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }

            if (bobVisual)
            {
                float newY = _startPosition.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected) return;

            // Verifica se quem colidiu é o jogador
            if (other.CompareTag("Player") || other.GetComponentInParent<StarterAssets.ThirdPersonController>() != null || other.GetComponent<CharacterController>() != null)
            {
                GameObject player = other.gameObject;
                _isCollected = true;

                string messageToDisplay = "";
                CanvasGroup targetCanvasGroup = textCanvasGroup;

                // 1. Desbloqueia Gancho
                if (unlockGrapple)
                {
                    GrappleHook grapple = player.GetComponent<GrappleHook>() ?? player.GetComponentInParent<GrappleHook>();
                    if (grapple != null) grapple.hasGrapple = true;
                    messageToDisplay = grappleUnlockMessage;
                    if (grappleCanvasGroup != null) targetCanvasGroup = grappleCanvasGroup;
                }

                // 2. Desbloqueia Super Pulo
                if (unlockSuperJump)
                {
                    SuperJump superJump = player.GetComponent<SuperJump>() ?? player.GetComponentInParent<SuperJump>();
                    if (superJump != null) superJump.hasSuperJump = true;
                    messageToDisplay = superJumpUnlockMessage;
                    if (superJumpCanvasGroup != null) targetCanvasGroup = superJumpCanvasGroup;
                }

                // 3. Desbloqueia Planar
                if (unlockGlide)
                {
                    GlideAbility glide = player.GetComponent<GlideAbility>() ?? player.GetComponentInParent<GlideAbility>();
                    if (glide != null) glide.hasGlide = true;
                    messageToDisplay = glideUnlockMessage;
                    if (glideCanvasGroup != null) targetCanvasGroup = glideCanvasGroup;
                }

                // Atualiza o texto do TextMeshProUGUI se estiver atribuído
                if (unlockText != null && !string.IsNullOrEmpty(messageToDisplay))
                {
                    unlockText.text = messageToDisplay;
                }

                // Efeitos visuais e sonoros de coleta
                if (pickupVFX != null)
                {
                    Instantiate(pickupVFX, transform.position, Quaternion.identity);
                }

                if (pickupSFX != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSFX, transform.position);
                }

                // Desativa colisão e renderers do coletável no cenário (para parecer destruído imediatamente)
                Collider col = GetComponent<Collider>();
                if (col != null) col.enabled = false;

                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                {
                    r.enabled = false;
                }

                // Inicia a sequência de animação UI (Fade In 1s -> Permanece 3s -> Fade Out 1s -> Destrói Objeto)
                StartCoroutine(CollectionSequence(targetCanvasGroup));
            }
        }

        private IEnumerator CollectionSequence(CanvasGroup canvasGroup)
        {
            if (canvasGroup != null)
            {
                canvasGroup.gameObject.SetActive(true);

                // Fade In: 1 segundo (opacity 0 a 1)
                float elapsed = 0f;
                float fadeDuration = 1.0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                    yield return null;
                }
                canvasGroup.alpha = 1.0f;

                // Permanece visível por 3 segundos
                yield return new WaitForSeconds(3.0f);

                // Fade Out: 1 segundo (opacity 1 a 0)
                elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(1.0f - (elapsed / fadeDuration));
                    yield return null;
                }
                canvasGroup.alpha = 0f;
            }

            Destroy(gameObject);
        }
    }
}
