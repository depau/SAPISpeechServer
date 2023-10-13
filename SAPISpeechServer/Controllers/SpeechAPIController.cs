using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Interop.SpeechLib;

namespace SAPISpeechServer.Controllers
{
    [ApiController]
    [Route("api/speech")]
    public class SpeechApiController : ControllerBase
    {
        [HttpGet("voices")]
        public IEnumerable<String> GetVoices()
        {
            SpVoice voice = new SpVoice();
            ISpeechObjectTokens voices = voice.GetVoices();
            var voiceNames = new List<String>();

            for (int i = 0; i < voices.Count; i++)
            {
                SpObjectToken token = voices.Item(i);
                try
                {
                    string name = token.GetAttribute("Name");
                    string vendor = token.GetAttribute("Vendor");
                    voiceNames.Add($"{vendor}/{name}");
                    Console.WriteLine("Voice: " + voiceNames.Last());
                }
                catch (COMException e)
                {
                    var error = DecodeSpeechApiError(e);
                    Console.Write($"Failed to retrieve voice info for index {i}/{voices.Count}: ");
                    if (error != null)
                    {
                        Console.WriteLine(error);
                    }
                    else
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

            return voiceNames;
        }

        [HttpGet("speak/{voiceVendor}/{voiceName}")]
        public IActionResult Speak(string voiceVendor, string voiceName, string text)
        {
            SpVoice voice = new SpVoice();
            SpMemoryStream memoryStream = new SpMemoryStreamClass();

            memoryStream.Format.Type = SpeechAudioFormatType.SAFT48kHz16BitMono;

            // Find the voice
            ISpeechObjectTokens voices = voice.GetVoices($"Name={voiceName};Vendor={voiceVendor}", string.Empty);
            if (voices.Count == 0)
            {
                return NotFound();
            }

            voice.Voice = voices.Item(0);

            // Speak it as WAV to memory
            voice.AudioOutputStream = memoryStream;
            try
            {
                // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                voice.Speak(text, SpeechVoiceSpeakFlags.SVSFlagsAsync | SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
                voice.WaitUntilDone(3000);
            }
            catch (COMException e)
            {
                var error = DecodeSpeechApiError(e);
                if (error != null)
                {
                    return BadRequest(error);
                }
                else
                {
                    throw;
                }
            }

            // Return the WAV
            var audioData = (byte[])memoryStream.GetData();
            var wavData = CreateWavData(audioData, memoryStream.Format.GetWaveFormatEx());

            return File(wavData, "audio/wav");
        }

        private static byte[] CreateWavData(byte[] audioData, SpWaveFormatEx audioFormat)
        {
            using (MemoryStream memStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memStream))
            {
                // Write WAV header
                writer.Write("RIFF".ToCharArray());
                writer.Write(audioData.Length + 36); // Chunk size
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16); // Sub-chunk size
                writer.Write((short)1); // Audio format (1 for PCM)
                writer.Write(audioFormat.Channels); // Number of channels
                writer.Write(audioFormat.SamplesPerSec); // Sample rate
                writer.Write(audioFormat.AvgBytesPerSec); // Byte rate
                writer.Write(audioFormat.BlockAlign); // Block align
                writer.Write(audioFormat.BitsPerSample); // Bits per sample
                writer.Write("data".ToCharArray());
                writer.Write(audioData.Length); // Data chunk size

                // Write audio data
                writer.Write(audioData);

                return memStream.ToArray();
            }
        }

        private string? DecodeSpeechApiError(COMException e)
        {
            // https://learn.microsoft.com/en-us/previous-versions/office/developer/speech-technologies/jj127491(v=msdn.10)
            switch (e.ErrorCode)
            {
                case -2147201023:
                    return "The object has not been properly initialized.";
                case -2147201022:
                    return "The object has already been initialized.";
                case -2147201021:
                    return "The caller has specified an unsupported format.";
                case -2147201020:
                    return "The caller has specified flags that are not valid for this operation.";
                case 282629:
                    return "The operation has reached the end of stream.";
                case -2147201018:
                    return "The wave device is busy.";
                case -2147201017:
                    return "The wave device is not supported.";
                case -2147201016:
                    return "The wave device is not enabled.";
                case -2147201015:
                    return "There is no wave driver installed.";
                case -2147201014:
                    return "The file must be Unicode.";
                case -2147201012:
                    return "The phrase ID specified does not exist or is out of range.";
                case -2147201011:
                    return "The caller provided a buffer too small to return a result.";
                case -2147201010:
                    return "Caller did not specify a format prior to opening a stream.";
                case -2147201009:
                    return
                        "The stream I/O was stopped by setting the audio object to the stopped state. This will be returned for both read and write streams.";
                case 282640:
                    return
                        "This will be returned only on input (read) streams when the stream is paused. Reads on paused streams will not block, and this return code indicates that all of the data has been removed from the stream.";
                case -2147201007:
                    return "The rule name passed to ActivateGrammar was not valid.";
                case -2147201006:
                    return "An exception was raised during a call to the current TTS driver.";
                case -2147201005:
                    return "An exception was raised during a call to an application sentence filter.";
                case -2147201004:
                    return
                        "In speech recognition, the current method cannot be performed while a grammar rule is active.";
                case 282645:
                    return "The operation was successful, but only with automatic stream format conversion.";
                case 282646:
                    return "There is currently no hypothesis recognition available.";
                case -2147201001:
                    return "Cannot create a new object instance for the specified object category.";
                case -2147200996:
                    return "A rule reference in a grammar was made to a named rule that was never defined.";
                case -2147200995:
                    return "A non-dynamic grammar rule that has no body.";
                case -2147200994:
                    return "The grammar compiler failed due to an internal state error.";
                case -2147200993:
                    return "An attempt was made to modify a non-dynamic rule.";
                case -2147200992:
                    return "A rule name was duplicated.";
                case -2147200991:
                    return "A resource name was duplicated for a given rule.";
                case -2147200990:
                    return "Too many grammars have been loaded.";
                case -2147200989:
                    return "Circular reference in import rules of grammars.";
                case -2147200988:
                    return "A rule reference to an imported grammar that could not be resolved.";
                case -2147200987:
                    return "The format of the WAV file is not supported.";
                case 282662:
                    return
                        "This success code indicates that an SR method called with the SPRIF_ASYNC flag is being processed. When it has finished processing, an SPFEI_ASYNC_COMPLETED event will be generated.";
                case -2147200985:
                    return
                        "A grammar rule was defined with a null path through the rule. That is, it is possible to satisfy the rule conditions with no words.";
                case -2147200984:
                    return
                        "It is not possible to change the current engine or input. This occurs if SelectEngine is called while a recognition context exists.";
                case -2147200983:
                    return "A rule exists with matching IDs (names) but different names (IDs).";
                case -2147200982:
                    return
                        "A grammar contains no top-level, dynamic, or exported rules. There is no possible way to activate or otherwise use any rule in this grammar.";
                case -2147200981:
                    return "Rule 'A' refers to a second rule 'B' which, in turn, refers to rule 'A'.";
                case 282668:
                    return "Parse path cannot be parsed given the currently active rules.";
                case -2147200979:
                    return "Parse path cannot be parsed given the currently active rules.";
                case -2147200978:
                    return "A marshaled remote call failed to respond.";
                case -2147200977:
                    return
                        "This will only be returned on input (read) streams when the stream is paused because the SR driver has not retrieved data recently.";
                case -2147200976:
                    return
                        "The result does not contain any audio, nor does the portion of the element chain of the result contain any audio.";
                case -2147200975:
                    return
                        "This alternate is no longer a valid alternate to the result it was obtained from. Returned from ISpPhraseAlt methods.";
                case -2147200974:
                    return
                        "The result does not contain any audio, nor does the portion of the element chain of the result contain any audio. Returned from ISpResult::GetAudio and ISpResult::SpeakAudio.";
                case -2147200973:
                    return "The XML format string for this RULEREF is not valid, for example not a GUID or REFCLSID.";
                case 282676:
                    return "The operation is not supported for stream input.";
                case -2147200971:
                    return "The operation is not valid for all but newly created application lexicons.";
                case 282679:
                    return "The word exists but without pronunciation.";
                case -2147200968:
                    return "An operation was attempted on a stream object that has been closed.";
                case -2147200967:
                    return "When enumerating items, the requested index is greater than the count of items.";
                case -2147200966:
                    return "The requested data item (such as as data key or value) was not found.";
                case -2147200965:
                    return "Audio state passed to SetState() is not valid.";
                case -2147200964:
                    return "A generic MMSYS error not caught by _MMRESULT_TO_HRESULT.";
                case -2147200963:
                    return "An exception was raised during a call to the marshaling code.";
                case -2147200962:
                    return "Attempt was made to manipulate a non-dynamic grammar.";
                case -2147200961:
                    return "Cannot add ambiguous property.";
                case -2147200960:
                    return "The key specified is not valid.";
                case -2147200959:
                    return "The token specified is not valid.";
                case -2147200958:
                    return "The xml parser failed due to bad syntax.";
                case -2147200957:
                    return "The XML parser failed to load a required resource (such as a voice or recognizer).";
                case -2147200956:
                    return "Attempted to remove registry data from a token that is already in use elsewhere.";
                case -2147200955:
                    return
                        "Attempted to perform an action on an object token that has had associated registry key deleted.";
                case -2147200954:
                    return
                        "The selected voice was registered as multi-lingual. The Speech Platform does not support multi-lingual registration.";
                case -2147200953:
                    return "Exported rules cannot refer directly or indirectly to a dynamic rule.";
                case -2147200952:
                    return "Error parsing an XML-format grammar.";
                case -2147200951:
                    return "Incorrect word format, probably due to incorrect pronunciation string.";
                case -2147200950:
                    return "Methods associated with active audio stream cannot be called unless stream is active.";
                case -2147200949:
                    return "Arguments or data supplied by the engine are not in a valid format or are inconsistent.";
                case -2147200948:
                    return "An exception was raised during a call to the current SR engine.";
                case -2147200947:
                    return "Stream position information supplied from engine is inconsistent.";
                case 282702:
                    return
                        "Operation could not be completed because the recognizer is inactive. It is inactive either because the recognition state is currently inactive or because no rules are active.";
                case -2147200945:
                    return "When making a remote call to the server, the call was made on the wrong thread.";
                case -2147200944:
                    return "The remote process terminated unexpectedly.";
                case -2147200943:
                    return "The remote process is already running; it cannot be started a second time.";
                case -2147200942:
                    return "An attempt to load a CFG grammar with a LANGID different than other loaded grammars.";
                case 282707:
                    return "A grammar-ending parse has been found that does not use all available words.";
                case -2147200940:
                    return "An attempt to deactivate or activate a non top-level rule.";
                case 282709:
                    return "An attempt to parse when no rule was active.";
                case -2147200938:
                    return "An attempt to ask a container lexicon for all words at once.";
                case 282711:
                    return "An attempt to activate a rule or grammar without calling SetInput first.";
                case -2147200935:
                    return "The requested language is not supported.";
                case -2147200934:
                    return "The operation cannot be performed because the voice is currently paused.";
                case -2147200933:
                    return
                        "This will only be returned on input (read) streams when the real time audio device stops returning data for a long period of time.";
                case -2147200932:
                    return
                        "An audio device stopped returning data from the Read() method even though it was in the run state. This error is only returned in the END_SR_STREAM event.";
                case -2147200931:
                    return
                        "The SR engine is unable to add this word to a grammar. The application may need to supply an explicit pronunciation for this word.";
                case -2147200930:
                    return
                        "An attempt to call ScaleAudio on a recognition result having previously called GetAlternates. Allowing the call to succeed would result in the previously created alternates located in incorrect audio stream positions.";
                case -2147200928:
                    return "A task could not complete because the SR engine had timed out.";
                case -2147200927:
                    return "An SR engine called synchronize while inside of a synchronize call.";
                case -2147200926:
                    return "The grammar contains a node no arcs.";
                case -2147200925:
                    return "Neither audio output nor input is supported for non-active console sessions.";
                case -2147200924:
                    return
                        "The object is a stale reference and is not valid to use. For example, having an ISpeechGrammarRule object reference and then calling ISpeechRecoGrammar::Reset() will cause the rule object to be invalidated. Calling any methods after this will result in this error.";
                case 282725:
                    return
                        "This can be returned from Read or Write calls for audio streams when the stream is stopped.";
                case -2147200922:
                    return
                        "The Recognition Parse Tree could not be generated. For example, a rule name begins with a digit but the XML parser does not allow an element name beginning with a digit.";
                case -2147200921:
                    return
                        "The SML could not be generated. For example, the transformation xslt template is not well formed.";
                case -2147200920:
                    return
                        "The SML could not be generated. For example, the transformation xslt template is not well formed.";
                case -2147200919:
                    return "There is already a root rule for this grammar. Defining another root rule will fail.";
                case -2147200912:
                    return
                        "Support for embedded script not supported because browser security settings have disabled it.";
                case -2147200911:
                    return "A time out occurred starting the sapi server.";
                case -2147200910:
                    return "A timeout occurred obtaining the lock for starting or connecting to sapi server.";
                case -2147200909:
                    return "When there is a cfg grammar loaded, changing the security manager is not permitted.";
                case 282740:
                    return "Parse is valid but could be extendable (internal use only).";
                case -2147200907:
                    return "Tried and failed to delete an existing file.";
                case -2147200905:
                    return "No recognizer is installed.";
                case -2147200904:
                    return "No audio device is installed.";
                case -2147200903:
                    return "No vowel in a word.";
                case -2147200902:
                    return "No vowel in a word.";
                case 282747:
                    return "The grammar does not have any root or top-level active rules to activate.";
                case 282748:
                    return "The engine does not need Speech Platform word entry handles for this grammar.";
                case -2147200899:
                    return "The word passed to the GetPronunciations interface needs normalizing first.";
                case -2147200898:
                    return "The word passed to the normalize interface cannot be normalized.";
                case -2147200896:
                    return "This combination of function call and input is currently not supported. ";
            }

            return null;
        }
    }
}