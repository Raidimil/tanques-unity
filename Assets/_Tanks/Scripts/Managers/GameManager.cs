using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int m_MaxLosses = 5;
    public float m_MaxGameTime = 240f;
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public CameraControl m_CameraControl;
    public Text m_MessageText;
    public Text m_TimerText;
    public GameObject m_GameOverPanel;
    public Text m_GameOverText;
    public GameObject m_TankPrefab;
    public TankManager[] m_Tanks;
    public GameObject m_EnemyPrefab;
    public Transform[] m_EnemySpawnPoints;

    private int m_RoundNumber;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;
    private bool m_GameOver;
    private bool m_TimeUp;
    private float m_TimeLeft;
    private List<GameObject> m_Enemies = new List<GameObject>();

    private void Start()
    {
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);
        m_TimeLeft = m_MaxGameTime;

        if (m_GameOverPanel != null)
            m_GameOverPanel.SetActive(false);
        UpdateTimerUI();

        SpawnAllTanks();
        SetCameraTargets();
        StartCoroutine(GameLoop());
    }

    private void SpawnAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance = Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }

    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = m_Tanks[i].m_Instance.transform;
        }
        m_CameraControl.m_Targets = targets;
    }

    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameOver)
            ShowGameOver();
        else
            StartCoroutine(GameLoop());
    }

    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();
        m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;
        yield return m_StartWait;
    }

    private IEnumerator RoundPlaying()
    {
        EnableTankControl();
        SpawnEnemies();
        m_MessageText.text = string.Empty;

        while (!OneTankLeft() && m_TimeLeft > 0f)
        {
            m_TimeLeft -= Time.deltaTime;
            UpdateTimerUI();
            yield return null;
        }

        m_TimeLeft = Mathf.Max(0f, m_TimeLeft);
        UpdateTimerUI();
        m_TimeUp = !OneTankLeft();
    }

    private IEnumerator RoundEnding()
    {
        DisableTankControl();
        DestroyEnemies();
        m_RoundWinner = null;

        if (m_TimeUp)
        {
            m_GameOver = true;
        }
        else
        {
            m_RoundWinner = GetRoundWinner();
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                if (m_Tanks[i] == m_RoundWinner)
                    m_Tanks[i].m_Wins++;
                else
                    m_Tanks[i].m_Losses++;
            }
            CheckGameOver();
        }

        m_MessageText.text = m_TimeUp ? "¡SE ACABÓ EL TIEMPO!" : EndMessage();
        yield return m_EndWait;
    }

    private bool OneTankLeft()
    {
        int numTanksLeft = 0;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }
        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }
        return null;
    }

    private void CheckGameOver()
    {
        bool someoneOut = false;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Losses >= m_MaxLosses)
                someoneOut = true;
        }
        if (!someoneOut)
            return;

        m_GameOver = true;

        TankManager best = m_Tanks[0];
        bool tie = false;
        for (int i = 1; i < m_Tanks.Length; i++)
        {
            if (IsBetter(m_Tanks[i], best))
            {
                best = m_Tanks[i];
                tie = false;
            }
            else if (!IsBetter(best, m_Tanks[i]))
            {
                tie = true;
            }
        }
        m_GameWinner = tie ? null : best;
    }

    private bool IsBetter(TankManager a, TankManager b)
    {
        if (a.m_Losses != b.m_Losses)
            return a.m_Losses < b.m_Losses;
        return a.m_Wins > b.m_Wins;
    }

    private void ShowGameOver()
    {
        string text;
        if (m_TimeUp)
        {
            text = "¡TIEMPO AGOTADO!\n\nAMBOS JUGADORES PIERDEN\n\n" + ScoreLines();
        }
        else if (m_GameWinner != null)
        {
            int score = m_GameWinner.m_Wins * 100 + Mathf.RoundToInt(m_TimeLeft);
            text = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!\n\n"
                 + "Puntaje: " + score + "\n"
                 + "Tiempo: " + FormatTime(m_MaxGameTime - m_TimeLeft) + "\n\n"
                 + ScoreLines();
        }
        else
        {
            text = "EMPATE\n\nNadie gana el juego\n\n" + ScoreLines();
        }

        if (m_GameOverPanel != null && m_GameOverText != null)
        {
            m_GameOverText.text = text;
            m_GameOverPanel.SetActive(true);
        }
        else
        {
            m_MessageText.text = text;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateTimerUI()
    {
        if (m_TimerText == null)
            return;
        m_TimerText.text = FormatTime(m_TimeLeft);
        m_TimerText.color = m_TimeLeft <= 30f ? Color.red : Color.white;
    }

    private string FormatTime(float seconds)
    {
        int total = Mathf.Max(0, Mathf.RoundToInt(seconds));
        return string.Format("{0:00}:{1:00}", total / 60, total % 60);
    }

    private string EndMessage()
    {
        string message = "EMPATE!";
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";
        message += "\n\n\n\n" + ScoreLines();
        return message;
    }

    private string ScoreLines()
    {
        string s = "";
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            s += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " victorias | "
               + m_Tanks[i].m_Losses + "/" + m_MaxLosses + " derrotas\n";
        }
        return s;
    }

    private void SpawnEnemies()
    {
        if (m_EnemyPrefab == null)
            return;
        for (int i = 0; i < m_EnemySpawnPoints.Length; i++)
        {
            m_Enemies.Add(Instantiate(m_EnemyPrefab, m_EnemySpawnPoints[i].position, m_EnemySpawnPoints[i].rotation));
        }
    }

    private void DestroyEnemies()
    {
        for (int i = 0; i < m_Enemies.Count; i++)
        {
            if (m_Enemies[i] != null)
                Destroy(m_Enemies[i]);
        }
        m_Enemies.Clear();
    }

    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].Reset();
    }

    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].EnableControl();
    }

    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].DisableControl();
    }
}