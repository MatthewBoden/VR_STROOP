using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class JsonSerializer : MonoBehaviour
{
	/// <summary>
	/// Loads a file from the given filename and returns it as a generic type
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="filename"></param>
	/// <returns></returns>
    public static T Load<T>(string filename) where T: class
	{
		if (File.Exists(filename))
		{
			try
			{
				using (Stream stream = File.OpenRead(filename))
				{
                    Dictionary<string, object> data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(File.ReadAllText(filename));
                    string json = MiniJSON.Json.Serialize(data);
                    // Debug.Log(json);
                    return data as T;
				}
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
			}
		}
		return default(T);
	}

	public static T LoadList<T>(string filename) where T: class
    {
        if (File.Exists(filename))
        {
            try
            {
                using (Stream stream = File.OpenRead(filename))
                {
                    List<object> data = (List<object>)MiniJSON.Json.Deserialize(File.ReadAllText(filename));
                    string json = MiniJSON.Json.Serialize(data);
                    // Debug.Log(json);
                    return data as T;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        return default(T);
    }

	/// <summary>
	/// Saves the given data to the given filename
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="filename"></param>
	/// <param name="data"></param>
	public static void Save<T>(string filename, T data) where T: class
	{
		using (Stream stream = File.OpenWrite(filename))
		{	
            string json = MiniJSON.Json.Serialize(data);
            // Debug.Log(json);
            byte[] jsonBytes = System.Text.Encoding.ASCII.GetBytes(json);
            stream.Write(jsonBytes, 0, jsonBytes.Length);
		}
	}
}
