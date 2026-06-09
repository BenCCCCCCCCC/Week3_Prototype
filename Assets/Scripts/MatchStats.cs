using System;

[Serializable]
public class MatchStats
{
    public bool escaped;
    public bool eliminated;

    public int completedCipherCount;
    public int gateOpenCount;
    public int rescueCount;
    public int rescueAttemptCount;

    public int hunterHitCount;
    public int survivorHitTakenCount;
    public int downCount;
    // -1f means no first down recorded in this match.
    public float firstDownTime = -1f;

    public int environmentInteractCount;

    public float totalRepairProgress;
    public float surviveTime;
}
