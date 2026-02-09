using System;
using UnityEngine;

public class PEEvent
{
    public static Action<Character, int> OnCharacterGainedBlock;    // int는 Gain 후의 Block값.
    public static Action<Character, int> OnCharacterLostBlock;      // int는 Lost 후의 Block값.
    public static Action<bool, Character, string> OnSetDialogueBubble;
}
