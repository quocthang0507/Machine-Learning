using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Program
{
    private const string ApiUri = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0";
    private const string SubscriptionKey = "412c26a6b7444f928258b43880fb2854";
    private const string PersonGroupId = "my-group-id";
    private static readonly HttpClient Client = GetClient();

    static void Main(string[] args)
    {
        Console.WriteLine("** Start face identification **"); Console.WriteLine();
        Console.WriteLine();

        AddPeopleWithFacesToGroup();
        PersonsGroupInfo.ForEach(person =>
        {
            Console.WriteLine($"Added person: {person.Name}");
            Console.WriteLine($"Person Id: {person.PersonId}");
            Console.WriteLine($"Face image URL: {person.FaceImageUrl}");
            Console.WriteLine();
        });

        // Train the Group

        // Detect and Identify

        Console.WriteLine("** End face identification **"); Console.WriteLine();
    }

    private static void CreatePersonGroup()
    {
        var body = JsonConvert.SerializeObject(new
        {
            name = PersonGroupId,
            userData = string.Empty
        });
        using (var content = new ByteArrayContent(Encoding.UTF8.GetBytes(body)))
        {
            using (var httpResponse = Client.PutAsync($"{ApiUri}/persongroups/{PersonGroupId}", content).Result)
            {
                var responseContent = DeserializeResponse(httpResponse.Content.ReadAsStringAsync().Result);
                if (httpResponse.IsSuccessStatusCode || responseContent?.Error?.Code == "PersonGroupExists")
                {
                    Console.WriteLine($"Group Created with Id: {PersonGroupId}");
                }
                else
                {
                    Console.WriteLine($"There was an error creating the group: {responseContent.Error.Message}");
                }
            }
        }
    }

    private static string CreatePerson(string name)
    {
        var body = JsonConvert.SerializeObject(new
        {
            name,
            userData = string.Empty
        });
        using (var content = new ByteArrayContent(Encoding.UTF8.GetBytes(body)))
        {
            using (var httpResponse = Client.PostAsync($"{ApiUri}/persongroups/{PersonGroupId}/persons", content).Result)
            {
                httpResponse.EnsureSuccessStatusCode();
                var json = httpResponse.Content.ReadAsStringAsync().Result;
                var createdPersonInformation = DeserializePersonInformation(json);
                return createdPersonInformation.PersonId;
            }
        }
    }

    private static string RegisterFace(string personId, string faceImageUrl)
    {
        var body = JsonConvert.SerializeObject(new
        {
            Url = faceImageUrl
        });
        using (var content = new ByteArrayContent(Encoding.UTF8.GetBytes(body)))
        {
            using (var httpResponse = Client.PostAsync($"{ApiUri}/persongroups/{PersonGroupId}/persons/{personId}/persistedFaces", content).Result)
            {
                httpResponse.EnsureSuccessStatusCode();
                var json = httpResponse.Content.ReadAsStringAsync().Result;
                var createdPersonInformation = DeserializePersonInformation(json);
                return createdPersonInformation.PersistedFaceId;
            }
        }
    }

    private static void AddPeopleWithFacesToGroup()
    {
        // Add people to the person group
        foreach (var person in PersonsGroupInfo)
        {
            var personId = CreatePerson(person.Name);
            person.PersonId = personId;
            // Register face to person
            var persistedFaceId = RegisterFace(personId, person.FaceImageUrl);
            person.PersistedFaceId = persistedFaceId;
        }
    }

    // TrainingGroupMethods

    // DetectMethod

    // IdentifyMethod

    private static HttpClient GetClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
        client.DefaultRequestHeaders.Add("ContentType", "application/json");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        return client;
    }

    public static readonly List<PersonInformation> PersonsGroupInfo = new List<PersonInformation>
  {
    new PersonInformation
    {
      Name = "Dad",
      FaceImageUrl = "https://aiplatformsprodstoragecdn.azureedge.net/aiplatformassets/knovuecdl43j/7rcwUbchvUgAEwuoCQoGkW/d0101ed8fab11d61aec225f799c23438/Family1-Dad1.jpg",
      PersonId = "PersonIdHere"
    },
    new PersonInformation
    {
      Name = "Son",
      FaceImageUrl = "https://aiplatformsprodstoragecdn.azureedge.net/aiplatformassets/knovuecdl43j/1lC6kB30naAiuqQ6uwYOUS/93e07f2c4da4e0a88fd51e73ca9e9ed6/Family1-Son1.jpg",
      PersonId = "PersonIdHere"
    },
    new PersonInformation
    {
      Name = "Daughter",
      FaceImageUrl = "https://aiplatformsprodstoragecdn.azureedge.net/aiplatformassets/knovuecdl43j/6NvVAbt2Qoc0uECICsK8i6/c0c900fdc5d700f9a2e415f17ce6ce89/Family1-Daughter2.jpg",
      PersonId = "PersonIdHere"
    }
  };

    public class PersonInformation
    {
        public PersonInformation() { }
        public PersonInformation(string name, string imageUrl, string personId)
        {
            Name = name;
            FaceImageUrl = imageUrl;
            PersonId = personId;
        }
        public string PersonId { get; set; }
        public string Name { get; set; }
        public string PersistedFaceId { get; set; }
        public string FaceImageUrl { get; set; }
    }

    public class TrainingStatus
    {
        public string Status { get; set; }
        public string CreatedDateTime { get; set; }
    }

    public class FaceDetectResponse
    {
        public string FaceId { get; set; }
    }

    public class Candidate
    {
        public string PersonId { get; set; }
        public double Confidence { get; set; }
    }

    public class FaceIdentificationResult
    {
        public string FaceId { get; set; }
        public List<Candidate> Candidates { get; set; }
    }

    public class Response
    {
        public Error Error { get; set; }
    }

    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }

    public static Response DeserializeResponse(string json)
    {
        return JsonConvert.DeserializeObject<Response>(json);
    }

    public static PersonInformation DeserializePersonInformation(string json)
    {
        return JsonConvert.DeserializeObject<PersonInformation>(json);
    }
    public static TrainingStatus DeserializeTrainingStatus(string json)
    {
        return JsonConvert.DeserializeObject<TrainingStatus>(json);
    }

    public static FaceDetectResponse[] DeserializeFaceDetectResponse(string json)
    {
        return JsonConvert.DeserializeObject<FaceDetectResponse[]>(json);
    }

    public static FaceIdentificationResult[] DeserializeFaceIdentificationResult(string json)
    {
        return JsonConvert.DeserializeObject<FaceIdentificationResult[]>(json);
    }
}