using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace SancaBGSPlatformer
{
    /// <summary>
    /// Gerenciador do Timer de Speedrun e Recordes do Jogo.
    /// Armazena o melhor tempo e o nome do detentor do recorde no PlayerPrefs.
    /// </summary>
    public class SpeedrunTimer : MonoBehaviour
    {
        public static SpeedrunTimer Instance { get; private set; }

        [Header("Referências da Interface (UI)")]
        [Tooltip("Texto TextMeshProUGUI que exibe o tempo atual da corrida.")]
        public TextMeshProUGUI currentTimeText;

        [Tooltip("Texto TextMeshProUGUI que exibe APENAS o tempo do recorde.")]
        public TextMeshProUGUI bestTimeText;

        [Tooltip("Texto TextMeshProUGUI que exibe APENAS o nome do detentor do recorde.")]
        public TextMeshProUGUI bestNameText;

        [Header("Configurações do PlayerPrefs")]
        [Tooltip("Chave para salvar/carregar o melhor tempo.")]
        public string playerPrefsTimeKey = "Speedrun_BestTime";

        [Tooltip("Chave para salvar/carregar o nome do jogador do melhor tempo.")]
        public string playerPrefsNameKey = "Speedrun_BestName";

        [Header("Estado do Cronômetro")]
        public bool isTimerRunning = false;
        public float currentTime = 0.0f;

        private float _bestTime = float.MaxValue;
        private string _bestPlayerName = "";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            LoadBestTime();
            UpdateTimerTextDisplay();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }

        private void Update()
        {
            if (isTimerRunning)
            {
                currentTime += Time.deltaTime;
                UpdateTimerTextDisplay();
            }
        }

        /// <summary>
        /// Inicia ou reinicia a contagem do cronômetro.
        /// </summary>
        public void StartTimer()
        {
            currentTime = 0.0f;
            isTimerRunning = true;
            Debug.Log("[SpeedrunTimer] Cronômetro INICIADO!");
        }

        /// <summary>
        /// Para o cronômetro e verifica se o tempo atual superou o recorde anterior.
        /// </summary>
        public bool FinishTimer()
        {
            if (!isTimerRunning) return false;

            isTimerRunning = false;
            Debug.Log($"[SpeedrunTimer] Cronômetro FINALIZADO! Tempo final: {FormatTime(currentTime)}");

            bool newRecord = (currentTime < _bestTime);
            if (newRecord)
            {
                Debug.Log($"[SpeedrunTimer] NOVO RECORDE ALCANÇADO: {FormatTime(currentTime)}!");
            }

            return newRecord;
        }

        /// <summary>
        /// Reseta a contagem atual do cronômetro.
        /// </summary>
        public void ResetTimer()
        {
            currentTime = 0.0f;
            isTimerRunning = false;
            UpdateTimerTextDisplay();
        }

        /// <summary>
        /// Carrega o recorde e o nome do jogador salvos no PlayerPrefs.
        /// </summary>
        public void LoadBestTime()
        {
            if (PlayerPrefs.HasKey(playerPrefsTimeKey))
            {
                _bestTime = PlayerPrefs.GetFloat(playerPrefsTimeKey);
                _bestPlayerName = PlayerPrefs.GetString(playerPrefsNameKey, "Jogador");
            }
            else
            {
                _bestTime = float.MaxValue;
                _bestPlayerName = "";
            }

            UpdateBestTimeTextDisplay();
        }

        /// <summary>
        /// Salva o tempo e o nome do jogador que bateu o recorde no PlayerPrefs.
        /// </summary>
        public void SaveBestTimeAndName(float timeInSeconds, string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Jogador";
            }

            _bestTime = timeInSeconds;
            _bestPlayerName = playerName;

            PlayerPrefs.SetFloat(playerPrefsTimeKey, _bestTime);
            PlayerPrefs.SetString(playerPrefsNameKey, _bestPlayerName);
            PlayerPrefs.Save();

            UpdateBestTimeTextDisplay();
            Debug.Log($"[SpeedrunTimer] Recorde salvo: {_bestPlayerName} - {FormatTime(_bestTime)}");
        }

        /// <summary>
        /// Apaga o recorde e nome salvos.
        /// </summary>
        public void ClearBestTimeRecord()
        {
            PlayerPrefs.DeleteKey(playerPrefsTimeKey);
            PlayerPrefs.DeleteKey(playerPrefsNameKey);
            _bestTime = float.MaxValue;
            _bestPlayerName = "";
            UpdateBestTimeTextDisplay();
        }

        private void UpdateTimerTextDisplay()
        {
            if (currentTimeText != null)
            {
                currentTimeText.text = FormatTime(currentTime);
            }
        }

        private void UpdateBestTimeTextDisplay()
        {
            // Atualiza o texto do tempo do recorde
            if (bestTimeText != null)
            {
                if (_bestTime == float.MaxValue)
                {
                    bestTimeText.text = "--:--.---";
                }
                else
                {
                    bestTimeText.text = FormatTime(_bestTime);
                }
            }

            // Atualiza o texto do nome do detentor do recorde
            if (bestNameText != null)
            {
                if (_bestTime == float.MaxValue || string.IsNullOrEmpty(_bestPlayerName))
                {
                    bestNameText.text = "---";
                }
                else
                {
                    bestNameText.text = _bestPlayerName;
                }
            }
        }

        /// <summary>
        /// Formata o tempo em segundos para o formato Minuto:Segundo.Milisegundos (mm:ss.fff).
        /// </summary>
        public static string FormatTime(float timeInSeconds)
        {
            if (timeInSeconds <= 0f || timeInSeconds == float.MaxValue)
                return "00:00.000";

            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000f) % 1000f);

            return string.Format("{0:D2}:{1:D2}.{2:D3}", minutes, seconds, milliseconds);
        }
    }
}
