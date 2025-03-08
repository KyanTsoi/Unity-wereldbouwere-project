using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;

public class ApiWorldClient : MonoBehaviour
{
    public TMP_InputField nameInput;
    public static ApiWorldClient instance { get; private set; }
    void Awake()
    {
        // hier controleren we of er al een instantie is van deze singleton
        // als dit zo is dan hoeven we geen nieuwe aan te maken en verwijderen we deze
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this);
    }
    private async Task<string> PerformApiCall(string url, string method, string jsonData = null, string token = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, method))
        {
            if (!string.IsNullOrEmpty(jsonData))
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError("Fout bij API-aanroep: " + request.error);
                return null;
            }
        }
    }
    public async void RegisterWorld()
    {
        if (SessionData.token != null)
        {
            var registerDto = new PostWorldRegisterRequestDto()
            {
                name = nameInput.text,
                ownerUserId = "<string>",
                maxLength = 200,
                maxHeight = 200
            };
            string jsonData = JsonUtility.ToJson(registerDto);
            var response = await PerformApiCall("https://avansict2228256.azurewebsites.net/wereldbouwer", "POST", jsonData, SessionData.token);
            Debug.Log(response);
        } else
        {
            Debug.LogError("SessionData token is null");
        }
        Debug.Log(SessionData.token);
        Debug.Log(nameInput.text);

    }

    public async void LoadWorld()
    {
        if (SessionData.token != null)
        {
            string url = $"https://avansict2228256.azurewebsites.net/wereldbouwer/getwereld/{SessionData.ownerUserId}";
            var response = await PerformApiCall(url, "GET", null, SessionData.token);

            try
            {
                var worldsResponse = JsonUtility.FromJson<GetWorldsResponseDto>("{\"worlds\":" + response + "}");
                if (worldsResponse != null && worldsResponse.worlds != null)
                {
                    foreach (var world in worldsResponse.worlds)
                    {
                        Debug.Log($"World ID: {world.id}, Name: {world.name}, Owner: {world.ownerUserId}");
                    }
                }
                else
                {
                    Debug.LogError("Failed to load worlds or no worlds found.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error deserializing JSON response: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("SessionData token is null");
        }
    }
}