using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Program
{
	private const string ApiUri = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0";
	private const string SubscriptionKey = "412c26a6b7444f928258b43880fb2854";
	private const string DadImage1 = "https://aiplatformsprodstoragecdn.azureedge.net/aiplatformassets/knovuecdl43j/7rcwUbchvUgAEwuoCQoGkW/d0101ed8fab11d61aec225f799c23438/Family1-Dad1.jpg";
	private const string DadImage2 = "https://aiplatformsprodstoragecdn.azureedge.net/aiplatformassets/knovuecdl43j/44CzBN7kGQaIkuU6Y2gc/b34ff0ba5651da7a6d6c2aae1cbf81d4/Family1-Dad2.jpg";
	private const string MomImage1 = "https://aiplatformsprodstoragecdn.azureedge.net/aiplatformassets/knovuecdl43j/3dH1Ddq1rWa08Oyaw66ASM/cacb0d8a1e4c7cd9fc698577a678ebc4/Family1-Mom1.jpg";
	private static readonly HttpClient Client = GetClient();

	static void Main(string[] args)
	{
		Console.WriteLine("Starting with the verify process");

		var dad1FaceId = Detect(DadImage1);
		var dad2FaceId = Detect(DadImage2);
		var mom1FaceId = Detect(MomImage1);

		// Verify same person faces

		// Verify different person faces

		Console.WriteLine("Ending with the verify process");
	}

	private static string Detect(string imageUrl)
	{
		var body = JsonConvert.SerializeObject(new
		{
			url = imageUrl
		});
		using (var content = new ByteArrayContent(Encoding.UTF8.GetBytes(body)))
		{
			using (var httpResponse = Client.PostAsync($"{ApiUri}/detect", content).Result)
			{
				httpResponse.EnsureSuccessStatusCode();
				var json = httpResponse.Content.ReadAsStringAsync().Result;
				var detectResponse = DeserializeFaceDetectResponse(json);
				foreach (var detectedFace in detectResponse)
				{
					Console.WriteLine($"Detected face with ID: {detectedFace.FaceId}");
				}
				return detectResponse.FirstOrDefault()?.FaceId;
			}
		}
	}

	// VerifyFacesMethod

	private static HttpClient GetClient()
	{
		var client = new HttpClient();
		client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
		client.DefaultRequestHeaders.Add("ContentType", "application/json");
		client.DefaultRequestHeaders.Add("Accept", "application/json");
		return client;
	}

	public class FaceDetectResponse
	{
		public string FaceId { get; set; }
	}

	public class FaceVerifyResponse
	{
		public bool IsIdentical { get; set; }
		public double Confidence { get; set; }
	}

	public static List<FaceDetectResponse> DeserializeFaceDetectResponse(string json)
	{
		return JsonConvert.DeserializeObject<List<FaceDetectResponse>>(json);
	}

	public static FaceVerifyResponse DeserializeFaceVerifyResponse(string json)
	{
		return JsonConvert.DeserializeObject<FaceVerifyResponse>(json);
	}
}
