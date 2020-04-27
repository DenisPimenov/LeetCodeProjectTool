using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LeetProjectCreator.Common;
using LeetProjectCreator.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeetProjectCreator
{
    public static class LeetCodeApi
    {
        private const string GraphQlApiUri = "https://leetcode.com/graphql";
        private const string AllProblemsUri = "https://leetcode.com/api/problems/all";

        private static readonly HttpClient HttpClient = new HttpClient();

        public static async Task<Result<ProblemData>> GetProblemData(string problemName)
        {
            var requestData = new
            {
                operationName = "questionData",
                variables = new
                {
                    titleSlug = problemName
                },
                query = QuestionRequest
            };

            var response = await HttpClient.PostAsync(GraphQlApiUri, CreateHttpContent(requestData));
            if (!response.IsSuccessStatusCode)
                return Errors.ProblemFetchError(problemName);

            var json = await response.Content.ReadAsStringAsync();
            var problem = JObject.Parse(json)
                .SelectToken("$.data.question")
                ?.ToObject<ProblemData>();
            if (problem == null)
                return Errors.ProblemFetchError(problemName);

            return problem;
        }

        public static async Task<Result<Problem[]>> GetAllProblems()
        {
            var response = await HttpClient.GetAsync(AllProblemsUri);
            if (!response.IsSuccessStatusCode)
                return Errors.ProblemsFetchError();

            var json = await response.Content.ReadAsStringAsync();
            var problems = JObject.Parse(json)
                .SelectToken("$.stat_status_pairs")
                ?.ToObject<Problem[]>();
            if (problems == null || problems.Length == 0)
                return Errors.ProblemsFetchError();

            return problems;
        }

        private static HttpContent CreateHttpContent<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private const string QuestionRequest = @"
query questionData($titleSlug: String!) 
{
    question(titleSlug: $titleSlug) 
    {
        questionId
        questionFrontendId
        boundTopicId    
        title
        titleSlug    
        content    
        translatedTitle    
        translatedContent    
        isPaidOnly   
        difficulty    
        likes
        dislikes    
        isLiked    
        similarQuestions    
        langToValidPlayground        
        companyTagStats    
        codeSnippets {
             lang    
             langSlug
              code
              __typename
        }
    }
}";
    }
}