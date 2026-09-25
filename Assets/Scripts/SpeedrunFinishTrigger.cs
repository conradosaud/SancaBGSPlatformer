using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace SancaBGSPlatformer
{
    /// <summary>
    /// Trigger de Encerramento e Tela de Final de Jogo.
    /// Ao colidir na chegada:
    /// - Para o cronômetro e exibe o tempo final.
    /// - Executa fade no CanvasGroup da tela final e habilita interactable/blocksRaycasts.
    /// - Desativa completamente o personagem.
    /// - Caso o recorde seja superado, habilita o painel de novo recorde para digitar o nome e salvar via botão.
    /// </summary>
    public class SpeedrunFinishTrigger : MonoBehaviour
    {
        [Tooltip("Referência opcional para o SpeedrunTimer. Se nulo, usará SpeedrunTimer.Instance.")]
        public SpeedrunTimer speedrunTimer;

        [Header("Tela de Final de Jogo (CanvasGroup)")]
        [Tooltip("CanvasGroup da interface de vitória/final de jogo.")]
        public CanvasGroup endGameCanvasGroup;

        [Tooltip("Duração em segundos do fade de opacidade da tela final.")]
        public float fadeDuration = 1.0f;

        [Tooltip("Texto TextMeshProUGUI que exibe o tempo final atingido pelo jogador.")]
        public TextMeshProUGUI finalTimeText;

        [Header("Painel de Novo Recorde")]
        [Tooltip("Container/GameObject ativado APENAS se o jogador bater o recorde.")]
        public GameObject newRecordContainer;

        [Tooltip("Campo de texto (TMP_InputField) para o jogador digitar seu nome.")]
        public TMP_InputField nameInputField;

        [Tooltip("Botão para confirmar e salvar o nome do jogador.")]
        public Button submitNameButton;

        [Header("Desativar Personagem")]
        [Tooltip("Se marcado, desativa completamente o GameObject do jogador ao terminar a partida.")]
        public bool disablePlayerOnFinish = true;

        [Header("Efeitos Visuais & Sonoros")]
        public GameObject victoryVFX;
        public AudioClip victorySFX;

        [Header("Reinício Automático (Opcional - Modo Kiosk)")]
        [Tooltip("Se marcado, reinicia a cena automaticamente após o tempo estipulado.")]
        public bool autoRestartScene = false;

        [Tooltip("Tempo em segundos para reiniciar a cena caso autoRestartScene esteja ligado.")]
        public float restartDelay = 10.0f;

        private bool _hasFinished = false;
        private float _finalTimeRecorded = 0.0f;

        private void Start()
        {
            // Garante que a tela final comece invisível e sem interação
            if (endGameCanvasGroup != null)
            {
                endGameCanvasGroup.alpha = 0f;
                endGameCanvasGroup.interactable = false;
                endGameCanvasGroup.blocksRaycasts = false;
            }

            if (newRecordContainer != null)
            {
                newRecordContainer.SetActive(false);
            }

            // Configura o listener do botão de salvar nome
            if (submitNameButton != null)
            {
                submitNameButton.onClick.AddListener(OnSubmitNameClicked);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasFinished) return;

            // Verifica se quem colidiu é o jogador
            if (other.CompareTag("Player") || other.GetComponentInParent<StarterAssets.ThirdPersonController>() != null || other.GetComponent<CharacterController>() != null)
            {
                SpeedrunTimer timer = speedrunTimer != null ? speedrunTimer : SpeedrunTimer.Instance;

                if (timer != null && timer.isTimerRunning)
                {
                    _hasFinished = true;
                    _finalTimeRecorded = timer.currentTime;

                    // 1. Encerra o cronômetro e verifica se foi batido o recorde
                    bool isNewRecord = timer.FinishTimer();

                    // 2. Desativa completamente o personagem
                    GameObject playerObj = other.gameObject;
                    if (disablePlayerOnFinish && playerObj != null)
                    {
                        // Desativa o GameObject raiz do jogador ou desabilita seus scripts
                        StarterAssets.ThirdPersonController tpc = playerObj.GetComponentInParent<StarterAssets.ThirdPersonController>();
                        if (tpc != null)
                        {
                            tpc.gameObject.SetActive(false);
                        }
                        else
                        {
                            playerObj.SetActive(false);
                        }
                    }

                    // Habilita e exibe o ponteiro do mouse para interação com os botões/inputs da UI
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    // 3. Exibe o tempo final no texto informado
                    if (finalTimeText != null)
                    {
                        finalTimeText.text = SpeedrunTimer.FormatTime(_finalTimeRecorded);
                    }

                    // 4. Lida com o contêiner de Novo Recorde
                    if (newRecordContainer != null)
                    {
                        newRecordContainer.SetActive(isNewRecord);
                    }

                    // Efeitos sonoros e visuais
                    if (victoryVFX != null)
                    {
                        Instantiate(victoryVFX, transform.position, Quaternion.identity);
                    }

                    if (victorySFX != null)
                    {
                        AudioSource.PlayClipAtPoint(victorySFX, transform.position);
                    }

                    // 5. Inicia o Fade do CanvasGroup da Tela Final
                    StartCoroutine(FadeEndGameScreen());

                    // Reinício automático da cena se ativado
                    if (autoRestartScene)
                    {
                        StartCoroutine(RestartSceneRoutine());
                    }
                }
            }
        }

        /// <summary>
        /// Corrotina que realiza o fade do CanvasGroup e ativa interactable e blocksRaycasts.
        /// </summary>
        private IEnumerator FadeEndGameScreen()
        {
            if (endGameCanvasGroup != null)
            {
                endGameCanvasGroup.gameObject.SetActive(true);

                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    endGameCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                    yield return null;
                }

                endGameCanvasGroup.alpha = 1.0f;
                endGameCanvasGroup.interactable = true;
                endGameCanvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// Chamado ao clicar no botão de confirmar nome do recorde.
        /// </summary>
        public void OnSubmitNameClicked()
        {
            SpeedrunTimer timer = speedrunTimer != null ? speedrunTimer : SpeedrunTimer.Instance;
            string playerName = nameInputField != null ? nameInputField.text.Trim() : "";

            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Jogador";
            }

            if (timer != null)
            {
                timer.SaveBestTimeAndName(_finalTimeRecorded, playerName);
            }

            // Desativa o botão ou oculta o container para feedback de confirmação
            if (submitNameButton != null)
            {
                submitNameButton.interactable = false;
            }

            if (newRecordContainer != null)
            {
                newRecordContainer.SetActive(false);
            }

            Debug.Log($"[SpeedrunFinishTrigger] Nome '{playerName}' salvo com sucesso no PlayerPrefs!");
        }

        /// <summary>
        /// Método público para reiniciar a partida (pode ser vinculado a um botão de Jogar Novamente).
        /// </summary>
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private IEnumerator RestartSceneRoutine()
        {
            yield return new WaitForSeconds(restartDelay);
            RestartGame();
        }

    }


}
