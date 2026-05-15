namespace HeadsetControl.NET;

/// <summary>
/// Position of a headset's chat-mix dial.
/// </summary>
/// <param name="Level">Raw dial position in the range 0-128. Below 64 favours game audio; above 64 favours chat audio.</param>
/// <param name="GameVolumePercent">Resulting game-audio volume in percent.</param>
/// <param name="ChatVolumePercent">Resulting chat-audio volume in percent.</param>
public readonly record struct ChatMixInfo(int Level, int GameVolumePercent, int ChatVolumePercent);
