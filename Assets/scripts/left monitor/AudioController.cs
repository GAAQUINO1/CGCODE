using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class AudioController : MonoBehaviour
{
	public static AudioController Instance { get; private set; }

	[Range(0f, 1f)]
	public float volume = 1f;

	private AudioSource audioSource;
	public bool finished = true;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.volume = volume;
	}

	public void ReadLine(string key, int index)
	{

		string path = Path.Combine(Application.streamingAssetsPath, $"AUDIO/{key}/{key}-{index.ToString("D2")}.wav");
		if (!File.Exists(path))
		{
			Debug.LogError($"!!Missing audio file: {path}");
			return;
		}

		StartCoroutine(PlayFromFile(path));
		finished = false;
	}

	IEnumerator PlayFromFile(string filePath)
	{
		string url = "file://" + filePath;
		using var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);

		yield return request.SendWebRequest();

		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError("Audio load error: " + request.error);
			yield break;
		}

		var clip = DownloadHandlerAudioClip.GetContent(request);
		audioSource.PlayOneShot(clip, volume);
		yield return new WaitForSeconds(clip.length);
		finished = true;
	}
}
