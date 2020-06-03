using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Detecting_faces_in_images
{
	class Program
	{
		//Replace ApiUri and Key when the API expired!
		private const string ApiUri = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0";
		private const string SubscriptionKey = "412c26a6b7444f928258b43880fb2854";
		private const string FaceVerificationImageUri = "https://azurecomcdn.azureedge.net/cvt-1a42997954ea75d7e789e26130078103e54a1124d1a98e4e3e72bd97f240fc65/images/shared/cognitive-services-demos/face-verification/3/verification-3.jpg";
		private static readonly HttpClient Client = GetClient();

		static void Main(string[] args)
		{
			Console.WriteLine("** Start face detection **");
			var detectResponse = Detect();
			for (int faceIndex = 0; faceIndex < detectResponse.Count(); faceIndex++)
			{
				var face = detectResponse[faceIndex];
				Console.WriteLine($"Face Detected #{faceIndex + 1}");
				Console.WriteLine($"  Id: {face.FaceId}");
				// Extra attributes
			};
			Console.WriteLine("** End face detection **");
			Console.WriteLine();
			Console.ReadKey();
		}

		private static FaceDetectResponse[] Detect()
		{
			var body = JsonConvert.SerializeObject(new
			{
				url = FaceVerificationImageUri
			});

			using (var content = new ByteArrayContent(Encoding.UTF8.GetBytes(body)))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				using (var httpResponse = Client.PostAsync($"{ApiUri}/detect", content).Result)
				{
					httpResponse.EnsureSuccessStatusCode();
					var json = httpResponse.Content.ReadAsStringAsync().Result;
					return DeserializeResponse(json);
				}
			}
		}

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
			public FaceAttributes FaceAttributes { get; set; }
			public FaceRectangle FaceRectangle { get; set; }
			public string FaceUrl { get; set; }
		}

		public class FaceAttributes
		{
			public string Gender { get; set; }
			public double Age { get; set; }
			public string Glasses { get; set; }
			public FacialHair FacialHair { get; set; }
			public Hair Hair { get; set; }

			private Dictionary<string, double> _emotion;
			public Dictionary<string, double> Emotion
			{
				get
				{
					return _emotion;
				}
				set
				{
					if (value != null && value.Count > 0)
					{
						// filter strongest emotion
						var top = value.OrderByDescending(pair => pair.Value).Take(1).First();
						_emotion = new Dictionary<string, double>();
						_emotion.Add(top.Key, top.Value);
					}
				}
			}
		}

		public class FacialHair
		{
			public double Moustache { get; set; }
			public double Beard { get; set; }
			public double Sideburns { get; set; }
		}

		public class HairColor
		{
			public string Color { get; set; }
			public double Confidence { get; set; }
		}

		public class Hair
		{
			public double Bald { get; set; }

			private List<HairColor> _hairColor;
			[JsonProperty("hairColor", NullValueHandling = NullValueHandling.Ignore)]
			public List<HairColor> HairColor
			{
				set
				{
					if (value != null && value.Count > 0)
					{
						// filter hair with highest score
						var top = value.OrderByDescending(entry => entry.Confidence).Take(1).First();
						_hairColor = new List<HairColor>();
						_hairColor.Add(top);
					}
				}
			}

			public string Color
			{
				get
				{
					return _hairColor?.First().Color;
				}
			}
		}

		public class FaceRectangle
		{
			public int Top { get; set; }
			public int Left { get; set; }
			public int Width { get; set; }
			public int Height { get; set; }
		}

		public static FaceDetectResponse[] DeserializeResponse(string json)
		{
			return JsonConvert.DeserializeObject<FaceDetectResponse[]>(json);
		}
	}
}
