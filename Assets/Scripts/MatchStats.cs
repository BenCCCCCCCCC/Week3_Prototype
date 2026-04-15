using System;

[Serializable]
public class MatchStats
{
    public bool escaped;
    public bool eliminated;

    public int completedCipherCount;
    public int gateOpenCount;
    public int rescueCount;

    public int hunterHitCount;
    public int survivorHitTakenCount;
    public int downCount;

    public int environmentInteractCount;

    public float totalRepairProgress;
    public float surviveTime;
}