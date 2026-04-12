using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;

namespace Whisper.Samples
{
    /// <summary>
    /// Stream transcription from microphone input.
    /// </summary>
    public class StreamingSampleMic : MonoBehaviour
    {
        [Header("Core")]
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;

        [Header("UI")]
        public Button button;
        public Text buttonText;
        public Text text;
        public ScrollRect scroll;

        [Header("Mic Button Visuals")]
        public Image buttonIconImage;
        public Sprite idleIcon;
        public Sprite recordingIcon;
        public float iconFadeDuration = 0.18f;

        [Header("Mic Status Text")]
        public Text micStatusText;
        public string idleStatusMessage = "Pulsa el micrófono para comenzar";
        public string listeningStatusMessage = "Escuchando...";

        private WhisperStream _stream;
        private Coroutine _iconTransitionCoroutine;

        private async void Start()
        {
            _stream = await whisper.CreateStream(microphoneRecord);
            _stream.OnResultUpdated += OnResult;
            _stream.OnSegmentUpdated += OnSegmentUpdated;
            _stream.OnSegmentFinished += OnSegmentFinished;
            _stream.OnStreamFinished += OnFinished;

            microphoneRecord.OnRecordStop += OnRecordStop;
            button.onClick.AddListener(OnButtonPressed);

            // Estado visual inicial
            UpdateButtonVisualImmediate(false);
            UpdateMicStatusText(false);
        }

        private void OnDestroy()
        {
            if (_stream != null)
            {
                _stream.OnResultUpdated -= OnResult;
                _stream.OnSegmentUpdated -= OnSegmentUpdated;
                _stream.OnSegmentFinished -= OnSegmentFinished;
                _stream.OnStreamFinished -= OnFinished;
            }

            if (microphoneRecord != null)
                microphoneRecord.OnRecordStop -= OnRecordStop;

            if (button != null)
                button.onClick.RemoveListener(OnButtonPressed);
        }

        private void OnButtonPressed()
        {
            if (!microphoneRecord.IsRecording)
            {
                _stream.StartStream();
                microphoneRecord.StartRecord();
            }
            else
            {
                microphoneRecord.StopRecord();
            }

            // Tras StartRecord / StopRecord ya se ha actualizado IsRecording
            bool isRecording = microphoneRecord.IsRecording;
            UpdateButtonVisualSmooth(isRecording);
            UpdateMicStatusText(isRecording);
        }

        private void OnRecordStop(AudioChunk recordedAudio)
        {
            UpdateButtonVisualSmooth(false);
            UpdateMicStatusText(false);
        }

        private void OnResult(string result)
        {
            text.text = result;
            UiUtils.ScrollDown(scroll);
        }

        private void OnSegmentUpdated(WhisperResult segment)
        {
            print($"Segment updated: {segment.Result}");
        }

        private void OnSegmentFinished(WhisperResult segment)
        {
            print($"Segment finished: {segment.Result}");
        }

        private void OnFinished(string finalResult)
        {
            print("Stream finished!");
        }

        private void UpdateButtonVisualImmediate(bool isRecording)
        {
            if (buttonText != null)
                buttonText.text = isRecording ? "Stop" : "Record";

            if (buttonIconImage == null)
                return;

            buttonIconImage.sprite = isRecording ? recordingIcon : idleIcon;

            Color c = buttonIconImage.color;
            c.a = 1f;
            buttonIconImage.color = c;
        }

        private void UpdateButtonVisualSmooth(bool isRecording)
        {
            if (buttonText != null)
                buttonText.text = isRecording ? "Stop" : "Record";

            if (buttonIconImage == null)
                return;

            if (_iconTransitionCoroutine != null)
                StopCoroutine(_iconTransitionCoroutine);

            _iconTransitionCoroutine = StartCoroutine(AnimateIconTransition(isRecording));
        }

        private IEnumerator AnimateIconTransition(bool isRecording)
        {
            if (buttonIconImage == null)
                yield break;

            float halfDuration = Mathf.Max(0.01f, iconFadeDuration * 0.5f);

            Color color = buttonIconImage.color;

            // Fade out
            float t = 0f;
            float startAlpha = color.a;
            while (t < halfDuration)
            {
                t += Time.deltaTime;
                float k = t / halfDuration;

                color.a = Mathf.Lerp(startAlpha, 0f, k);
                buttonIconImage.color = color;
                yield return null;
            }

            color.a = 0f;
            buttonIconImage.color = color;

            // Cambiar sprite
            buttonIconImage.sprite = isRecording ? recordingIcon : idleIcon;

            // Fade in
            t = 0f;
            while (t < halfDuration)
            {
                t += Time.deltaTime;
                float k = t / halfDuration;

                color.a = Mathf.Lerp(0f, 1f, k);
                buttonIconImage.color = color;
                yield return null;
            }

            color.a = 1f;
            buttonIconImage.color = color;

            _iconTransitionCoroutine = null;
        }

        private void UpdateMicStatusText(bool isRecording)
        {
            if (micStatusText == null)
                return;

            micStatusText.text = isRecording ? listeningStatusMessage : idleStatusMessage;
        }
    }
}