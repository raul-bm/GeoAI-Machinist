using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class FirestoreData
{
    public StringValue Nickname;
    public TimestampValue Date;
    public DoubleValue LevelDataLabeling;
    public DoubleValue LevelInput;
    public DoubleValue LevelConvolutional;
    public DoubleValue LevelActivation;
    public DoubleValue LevelPooling;
    public DoubleValue LevelOutput;
}

[Serializable] public class TimestampValue { public string timestampValue; }
[Serializable] public class StringValue { public string stringValue; }
[Serializable] public class DoubleValue { public double doubleValue; }

[Serializable]
public class FirestoreDocument
{
    public FirestoreData fields;
}

[Serializable]
public class AnonAuthResponse
{
    public string idToken;
    public string localId;
}

[System.Serializable]
public class EnvConfig
{
    public string firebaseApiKey;
    public string firebaseProjectId;
}

public class FirebaseManager : MonoBehaviour
{
    private string projectId = "";
    private string apiKey = "";

    private string idToken;
    private string userId;

    public static FirebaseManager instance;

    private string firestoreBaseUrl => $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents";
    private string authUrl => $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";

    private void Awake()
    {
        if (FirebaseManager.instance != null) Destroy(FirebaseManager.instance.gameObject);

        instance = this;
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        TextAsset jsonFile = Resources.Load<TextAsset>("env");
        if (jsonFile != null)
        {
            EnvConfig config = JsonUtility.FromJson<EnvConfig>(jsonFile.text);
            this.apiKey = config.firebaseApiKey;
            this.projectId = config.firebaseProjectId;
        }
        else
        {
            Debug.LogError(".env not found");
        }

        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        yield return SignInAnonymously();
        yield return UploadInitialData();

        // Example: update level 2 with 12.3 seconds
        //yield return UpdateLevelTime(2, 12.3f);
    }

    IEnumerator SignInAnonymously()
    {
        UnityWebRequest request = new UnityWebRequest(authUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes("{}");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var result = JsonUtility.FromJson<AnonAuthResponse>(request.downloadHandler.text);
            idToken = result.idToken;
            userId = result.localId;
            Debug.Log($"Anonymous sign-in successful. UID: {userId}");
        }
        else
        {
            Debug.LogError("Anonymous sign-in failed: " + request.downloadHandler.text);
        }
    }

    IEnumerator UploadInitialData()
    {
        string url = $"{firestoreBaseUrl}/time-collection/{userId}";

        var data = new FirestoreData
        {
            Nickname = new StringValue { stringValue = PlayerPrefs.GetString("nickname") },
            Date = new TimestampValue { timestampValue = DateTime.UtcNow.ToString("o") },
            LevelDataLabeling = new DoubleValue { doubleValue = 0 },
            LevelInput = new DoubleValue { doubleValue = 0 },
            LevelConvolutional = new DoubleValue { doubleValue = 0 },
            LevelActivation = new DoubleValue { doubleValue = 0 },
            LevelPooling = new DoubleValue { doubleValue = 0 },
            LevelOutput = new DoubleValue { doubleValue = 0 }
        };

        FirestoreDocument document = new FirestoreDocument { fields = data };
        string json = JsonUtility.ToJson(document);

        yield return PatchRequest(url, json);
    }

    public void UpdateLevel(string levelName, float time)
    {
        StartCoroutine(UpdateLevelTime(levelName, time));
    }

    public IEnumerator UpdateLevelTime(string levelName, float time)
    {
        string timeStr = time.ToString(System.Globalization.CultureInfo.InvariantCulture);

        string url = $"{firestoreBaseUrl}/time-collection/{userId}?updateMask.fieldPaths=Level{levelName}";
        string json = $"{{ \"fields\": {{ \"Level{levelName}\": {{ \"doubleValue\": {timeStr} }} }} }}";

        yield return PatchRequest(url, json);
    }

    IEnumerator PatchRequest(string url, string json)
    {
        var request = new UnityWebRequest(url, "PATCH");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + idToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Firestore update success:\n" + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Firestore error: " + request.error + "\n" + request.downloadHandler.text);
        }
    }
}

/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseManager : MonoBehaviour
{
    // Reemplaza estos valores por los de tu proyecto Firebase
    private const string apiKey = "AIzaSyCmZmpFzpvQg22eeWWdO5s9L8nb4fv4dws";
    private const string projectId = "geoaimachinist";

    private string idToken;
    private string localId;

    public static FirebaseManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(SignInAnonymously(() =>
        {
            Debug.Log("Usuario autenticado: " + localId);
            StartCoroutine(CrearDocumentoFirestore());
        }));
    }

    // ===========================
    // AUTENTICACIÓN ANÓNIMA
    // ===========================

    IEnumerator SignInAnonymously(Action onSuccess)
    {
        string authUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";
        string jsonPayload = "{\"returnSecureToken\": true}";

        using (UnityWebRequest request = UnityWebRequest.Put(authUrl, jsonPayload))
        {
            request.method = "POST";
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<FirebaseAuthResponse>(request.downloadHandler.text);
                idToken = response.idToken;
                localId = response.localId;

                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError("Error autenticando: " + request.error);
            }
        }
    }

    // ===========================
    // CREAR DOCUMENTO INICIAL
    // ===========================

    IEnumerator CrearDocumentoFirestore()
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/time-collection/{localId}";

        var data = new Dictionary<string, object>
        {
            ["fields"] = new Dictionary<string, object>
            {
                ["startTime"] = new Dictionary<string, string> { ["timestampValue"] = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'") },
                ["levelsTime"] = new Dictionary<string, object>
                {
                    ["level1"] = new Dictionary<string, double> { ["doubleValue"] = 0f }
                }
            }
        };

        string jsonPayload = JsonUtility.ToJson(data);

        using (UnityWebRequest request = UnityWebRequest.Put(url, jsonPayload))
        {
            request.method = "PATCH";
            request.SetRequestHeader("Authorization", $"Bearer {idToken}");
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Documento creado");
            }
            else
            {
                Debug.LogError("Error creando documento: " + request.error + " | " + request.downloadHandler.text);
            }
        }
    }

    // ===========================
    // ACTUALIZAR TIEMPO DE NIVEL
    // ===========================

    public void GuardarTiempoNivel(string nombreNivel, float duracion)
    {
        StartCoroutine(ActualizarTiempoNivel(nombreNivel, duracion));
    }

    IEnumerator ActualizarTiempoNivel(string nivel, float duracion)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/partidas/{localId}?updateMask.fieldPaths=niveles.{nivel}";

        var data = new
        {
            fields = new Dictionary<string, object>
            {
                {
                    "niveles", new
                    {
                        mapValue = new
                        {
                            fields = new Dictionary<string, object>
                            {
                                {
                                    nivel, new
                                    {
                                        mapValue = new
                                        {
                                            fields = new Dictionary<string, object>
                                            {
                                                { "duracion", new { doubleValue = duracion } }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        string jsonPayload = JsonUtility.ToJson(data);

        using (UnityWebRequest request = UnityWebRequest.Put(url, jsonPayload))
        {
            request.method = "PATCH";
            request.SetRequestHeader("Authorization", $"Bearer {idToken}");
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Tiempo del nivel '{nivel}' guardado: {duracion}s");
            }
            else
            {
                Debug.LogError("Error actualizando nivel: " + request.error + " | " + request.downloadHandler.text);
            }
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

[Serializable]
public class FirestoreData
{
    public TimestampValue StartTime;
    public DoubleValue Level1;
    public DoubleValue Level2;
    public DoubleValue Level3;
    public DoubleValue Level4;
    public DoubleValue Level5;
}

[Serializable] public class TimestampValue { public string timestampValue; }
[Serializable] public class DoubleValue { public double doubleValue; }

public class FirestoreUploader : MonoBehaviour
{
    private string projectId = "geoaimachinist";
    private string apiKey = "AIzaSyCmZmpFzpvQg22eeWWdO5s9L8nb4fv4dws";
    private string documentPath = "PlayerData/user_123"; // Collection/Document

    public void Start()
    {
        UploadInitialData();
    }

    public void UploadInitialData()
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/{documentPath}?key={apiKey}";

        var data = new FirestoreData
        {
            StartTime = new TimestampValue { timestampValue = DateTime.UtcNow.ToString("o") },
            Level1 = new DoubleValue { doubleValue = 0 },
            Level2 = new DoubleValue { doubleValue = 0 },
            Level3 = new DoubleValue { doubleValue = 0 },
            Level4 = new DoubleValue { doubleValue = 0 },
            Level5 = new DoubleValue { doubleValue = 0 }
        };

        string jsonBody = JsonUtility.ToJson(new { fields = data });

        StartCoroutine(PatchRequest(url, jsonBody));
    }

    public void UpdateLevelTime(int level, float time)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/{documentPath}?key={apiKey}&updateMask.fieldPaths=Level{level}";

        string json = $"{{ \"fields\": {{ \"Level{level}\": {{ \"doubleValue\": {time} }} }} }}";

        StartCoroutine(PatchRequest(url, json));
    }

    IEnumerator PatchRequest(string url, string json)
    {
        var request = new UnityWebRequest(url, "PATCH");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log("Firestore update success: " + request.downloadHandler.text);
        else
            Debug.LogError("Firestore error: " + request.error + "\n" + request.downloadHandler.text);
    }
}*/