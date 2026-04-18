using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum MatchResult
{
    None,
    SurvivorWin,
    HunterWin,
    Draw
}

public class MatchManager : MonoBehaviour
{
    [Header("Core Match Data")]
    public int requiredCompletedCiphers = 3;
    public int survivorWinEscapeCount = 1;
    public int hunterWinEliminationCount = 2;

    [Header("Endgame")]
    public float endgameDuration = 90f;
    public TMP_Text endgameCountdownText;

    [Header("Scene References")]
    public CipherMachine[] ciphers;
    public GateController[] gates;
    public GameObject[] trackedSurvivors;

    [Header("Gameplay References")]
    public RoleSwitchController roleSwitchController;
    public PlayerController hunterController;
    public InteractionUI hunterInteractionUI;
    public HunterSlowSkill hunterSlowSkill;
    public HunterDetectSkill hunterDetectSkill;
    public HunterBasicAttack hunterBasicAttack;
    public HunterCarryController hunterCarryController;

    public PlayerController survivorPlayerController;
    public InteractionUI survivorInteractionUI;
    public SurvivorDashSkill survivorDashSkill;

    [Header("Week 8")]
    public MatchSettlement matchSettlement;

    [Header("UI")]
    public ResultPanelUI resultPanelUI;

    [Header("Debug")]
    public bool logMatchEvents = true;

    private HashSet<CipherMachine> completedCipherSet = new HashSet<CipherMachine>();
    private HashSet<GameObject> escapedSurvivorSet = new HashSet<GameObject>();
    private HashSet<GameObject> eliminatedSurvivorSet = new HashSet<GameObject>();

    private float matchStartTime;
    private float matchEndTime;
    private float endgameRemainingTime = 0f;

    public int CompletedCipherCount => completedCipherSet.Count;
    public int EscapedSurvivorCount => escapedSurvivorSet.Count;
    public int EliminatedSurvivorCount => eliminatedSurvivorSet.Count;

    public int DownedCount { get; private set; } = 0;
    public bool GatesUnlocked { get; private set; } = false;
    public bool IsMatchEnded { get; private set; } = false;
    public bool IsEndgameActive { get; private set; } = false;
    public float EndgameRemainingTime => endgameRemainingTime;
    public MatchResult FinalResult { get; private set; } = MatchResult.None;

    void Start()
    {
        if (requiredCompletedCiphers < 1)
        {
            requiredCompletedCiphers = 1;
        }

        if (survivorWinEscapeCount < 1)
        {
            survivorWinEscapeCount = 1;
        }

        if (hunterWinEliminationCount < 1)
        {
            hunterWinEliminationCount = 1;
        }

        if (endgameDuration < 1f)
        {
            endgameDuration = 90f;
        }

        if (ciphers == null)
        {
            ciphers = new CipherMachine[0];
        }

        if (gates == null)
        {
            gates = new GateController[0];
        }

        if (trackedSurvivors == null)
        {
            trackedSurvivors = new GameObject[0];
        }

        matchStartTime = Time.time;
        matchEndTime = 0f;
        endgameRemainingTime = 0f;
        IsMatchEnded = false;
        IsEndgameActive = false;
        FinalResult = MatchResult.None;
        GatesUnlocked = false;
        DownedCount = 0;

        completedCipherSet.Clear();
        escapedSurvivorSet.Clear();
        eliminatedSurvivorSet.Clear();

        if (resultPanelUI != null)
        {
            resultPanelUI.HidePanel();
        }

        if (MatchStatsManager.Instance != null)
        {
            MatchStatsManager.Instance.StartMatch();
        }
        else if (logMatchEvents)
        {
            Debug.LogWarning("MatchManager: MatchStatsManager not found in scene.");
        }

        UpdateEndgameCountdownUI();

        if (logMatchEvents)
        {
            Debug.Log("MatchManager: Started. Required completed ciphers = " + requiredCompletedCiphers);
        }
    }

    void Update()
    {
        if (IsMatchEnded) return;
        if (!IsEndgameActive) return;

        endgameRemainingTime -= Time.deltaTime;

        if (endgameRemainingTime < 0f)
        {
            endgameRemainingTime = 0f;
        }

        UpdateEndgameCountdownUI();

        if (endgameRemainingTime <= 0f)
        {
            ResolveEndgameTimeout();
        }
    }

    public void OnCipherCompleted(CipherMachine cipher)
    {
        if (IsMatchEnded) return;
        if (cipher == null) return;

        if (completedCipherSet.Contains(cipher))
        {
            return;
        }

        completedCipherSet.Add(cipher);

        if (MatchStatsManager.Instance != null)
        {
            MatchStatsManager.Instance.SetCompletedCipherCount(CompletedCipherCount);
        }

        if (logMatchEvents)
        {
            Debug.Log("MatchManager: Cipher completed. Current count = " + CompletedCipherCount + "/" + requiredCompletedCiphers);
        }

        if (!GatesUnlocked && CompletedCipherCount >= requiredCompletedCiphers)
        {
            UnlockAllGates();
        }
    }

    void UnlockAllGates()
    {
        GatesUnlocked = true;

        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] != null)
            {
                gates[i].UnlockGate();
            }
        }

        if (logMatchEvents)
        {
            Debug.Log("MatchManager: All required ciphers completed. Gates unlocked.");
        }
    }

    public void OnGateOpened(GateController gate)
    {
        if (IsMatchEnded) return;
        if (IsEndgameActive) return;

        IsEndgameActive = true;
        endgameRemainingTime = endgameDuration;

        if (MatchStatsManager.Instance != null)
        {
            MatchStatsManager.Instance.AddGateOpen();
        }

        UpdateEndgameCountdownUI();

        if (logMatchEvents)
        {
            string gateName = gate != null ? gate.name : "Unknown Gate";
            Debug.Log("MatchManager: Endgame countdown started by gate = " + gateName + ", duration = " + endgameDuration);
        }
    }

    void ResolveEndgameTimeout()
    {
        if (IsMatchEnded) return;
        if (!IsEndgameActive) return;

        IsEndgameActive = false;
        endgameRemainingTime = 0f;
        UpdateEndgameCountdownUI();

        if (trackedSurvivors != null)
        {
            for (int i = 0; i < trackedSurvivors.Length; i++)
            {
                GameObject survivor = trackedSurvivors[i];
                if (survivor == null) continue;
                if (escapedSurvivorSet.Contains(survivor)) continue;
                if (eliminatedSurvivorSet.Contains(survivor)) continue;

                CharacterStatus status = survivor.GetComponent<CharacterStatus>();
                if (status != null && !status.IsEscaped && !status.IsEliminated)
                {
                    status.Eliminate();
                }

                eliminatedSurvivorSet.Add(survivor);

                if (logMatchEvents)
                {
                    Debug.Log("MatchManager: Endgame timeout auto-failed survivor = " + survivor.name);
                }
            }
        }

        EndMatch(MatchResult.HunterWin);
    }

    public void OnSurvivorDowned(GameObject survivor)
    {
        if (IsMatchEnded) return;
        if (survivor == null) return;

        DownedCount++;

        if (MatchStatsManager.Instance != null)
        {
            MatchStatsManager.Instance.AddDown();
        }

        if (logMatchEvents)
        {
            Debug.Log("MatchManager: Survivor downed = " + survivor.name + ". Downed count = " + DownedCount);
        }
    }

    public void OnSurvivorEscaped(GameObject survivor)
    {
        if (IsMatchEnded) return;
        if (survivor == null) return;

        if (escapedSurvivorSet.Contains(survivor))
        {
            return;
        }

        escapedSurvivorSet.Add(survivor);

        CharacterStatus status = survivor.GetComponent<CharacterStatus>();
        if (status != null)
        {
            status.MarkEscaped();
        }

        if (logMatchEvents)
        {
            Debug.Log("MatchManager: Survivor escaped = " + survivor.name + ". Escaped count = " + EscapedSurvivorCount);
        }

        EvaluateMatchEnd();
    }

    public void OnSurvivorEliminated(GameObject survivor)
    {
        if (IsMatchEnded) return;
        if (survivor == null) return;

        if (eliminatedSurvivorSet.Contains(survivor))
        {
            return;
        }

        eliminatedSurvivorSet.Add(survivor);

        if (logMatchEvents)
        {
            Debug.Log("MatchManager: Survivor eliminated = " + survivor.name + ". Eliminated count = " + EliminatedSurvivorCount);
        }

        EvaluateMatchEnd();
    }

    void EvaluateMatchEnd()
    {
        if (IsMatchEnded) return;

        int resolvedCount = EscapedSurvivorCount + EliminatedSurvivorCount;
        int totalTracked = trackedSurvivors != null ? trackedSurvivors.Length : 0;

        if (EscapedSurvivorCount >= survivorWinEscapeCount)
        {
            EndMatch(MatchResult.SurvivorWin);
            return;
        }

        if (EliminatedSurvivorCount >= hunterWinEliminationCount)
        {
            EndMatch(MatchResult.HunterWin);
            return;
        }

        if (resolvedCount >= totalTracked && totalTracked > 0)
        {
            if (EscapedSurvivorCount > EliminatedSurvivorCount)
            {
                EndMatch(MatchResult.SurvivorWin);
            }
            else if (EliminatedSurvivorCount > EscapedSurvivorCount)
            {
                EndMatch(MatchResult.HunterWin);
            }
            else
            {
                EndMatch(MatchResult.Draw);
            }
        }
    }

    void EndMatch(MatchResult result)
    {
        if (IsMatchEnded) return;

        IsMatchEnded = true;
        IsEndgameActive = false;
        FinalResult = result;
        matchEndTime = Time.time;
        endgameRemainingTime = 0f;

        UpdateEndgameCountdownUI();
        FreezeGameplay();

        if (MatchStatsManager.Instance != null)
        {
            MatchStatsManager.Instance.SetCompletedCipherCount(CompletedCipherCount);

            bool escaped = EscapedSurvivorCount > 0;
            bool eliminated = EliminatedSurvivorCount > 0;

            MatchStatsManager.Instance.EndMatch(escaped, eliminated);
        }

        if (matchSettlement != null)
        {
            matchSettlement.SettleMatch();
        }

        if (resultPanelUI != null)
        {
            float duration = matchEndTime - matchStartTime;
            resultPanelUI.ShowResults(
                result,
                duration,
                EscapedSurvivorCount,
                EliminatedSurvivorCount,
                DownedCount,
                CompletedCipherCount
            );
        }

        if (logMatchEvents)
        {
            Debug.Log("MatchManager: Match ended. Result = " + result);
        }
    }

    void FreezeGameplay()
    {
        if (roleSwitchController != null)
        {
            roleSwitchController.enabled = false;
        }

        if (hunterController != null)
        {
            hunterController.SetPlayerInputEnabled(false);
        }

        if (survivorPlayerController != null)
        {
            survivorPlayerController.SetPlayerInputEnabled(false);
        }

        if (hunterInteractionUI != null)
        {
            hunterInteractionUI.enabled = false;
        }

        if (survivorInteractionUI != null)
        {
            survivorInteractionUI.enabled = false;
        }

        if (hunterSlowSkill != null)
        {
            hunterSlowSkill.enabled = false;
        }

        if (hunterDetectSkill != null)
        {
            hunterDetectSkill.enabled = false;
        }

        if (hunterBasicAttack != null)
        {
            hunterBasicAttack.enabled = false;
        }

        if (hunterCarryController != null)
        {
            hunterCarryController.enabled = false;
        }

        if (survivorDashSkill != null)
        {
            survivorDashSkill.enabled = false;
        }

        CipherMachine[] allCiphers = FindObjectsByType<CipherMachine>(FindObjectsSortMode.None);
        for (int i = 0; i < allCiphers.Length; i++)
        {
            if (allCiphers[i] != null)
            {
                allCiphers[i].enabled = false;
            }
        }

        GateController[] allGates = FindObjectsByType<GateController>(FindObjectsSortMode.None);
        for (int i = 0; i < allGates.Length; i++)
        {
            if (allGates[i] != null)
            {
                allGates[i].enabled = false;
            }
        }

        ChairController[] allChairs = FindObjectsByType<ChairController>(FindObjectsSortMode.None);
        for (int i = 0; i < allChairs.Length; i++)
        {
            if (allChairs[i] != null)
            {
                allChairs[i].enabled = false;
            }
        }
    }

    void UpdateEndgameCountdownUI()
    {
        if (endgameCountdownText == null) return;

        if (!IsEndgameActive || IsMatchEnded)
        {
            endgameCountdownText.gameObject.SetActive(false);
            endgameCountdownText.text = "";
            return;
        }

        endgameCountdownText.gameObject.SetActive(true);

        int totalSeconds = Mathf.CeilToInt(endgameRemainingTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        endgameCountdownText.text = "Endgame: " + minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public bool IsCipherGoalReached()
    {
        return CompletedCipherCount >= requiredCompletedCiphers;
    }
}