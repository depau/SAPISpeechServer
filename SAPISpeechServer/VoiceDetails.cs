namespace SAPISpeechServer;

/// <summary>
/// An object that provides information about a voice.
/// </summary>
public class VoiceDetails
{
    /// <summary>
    /// The name of the voice.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The name of the voice vendor.
    /// </summary>
    public string? Vendor { get; set; }

    /// <summary>
    /// The language code of the voice.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// The gender of this voice.
    /// </summary>
    public string? Gender { get; set; }
}