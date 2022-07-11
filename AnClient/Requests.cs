using System;
using System.IO;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace AnClient {
    class Requests {

        internal static void AnServer_Post<T>(T payload, string question = "") {
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(payload, Program.JSON_SERIALIZER_OPTIONS_DEFAULT);

            HttpContent hcontent = new ByteArrayContent(json);
            hcontent.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            var response = Program.httpClient.PostAsync($"{Program.anServer_url}?question={HttpUtility.UrlEncode(question)}", hcontent);
            try {
                response.Wait();
            } catch {
                throw new AnServer_request_URLException($"URL ERROR: {Program.anServer_url}?question={question}");
            }

            if (!response.Result.IsSuccessStatusCode) {
                throw new AnServer_request_RecvCodeException($"ERROR: {response.Result.StatusCode} {Program.anServer_url}?question={question}", response.Result.StatusCode);
            }
        }

        internal static T AnServer_rangebleRequest<T>(int startindex, int count, string question = "") {
            var response = Program.httpClient.GetAsync($"{Program.anServer_url}?question={HttpUtility.UrlEncode(question)}&startindex={startindex}&count={count}");
            try {
                response.Wait();
            } catch {
                throw new AnServer_request_URLException($"URL ERROR: {Program.anServer_url}?question={question}&startindex={startindex}&count={count}");
            }

            if (!response.Result.IsSuccessStatusCode) {
                throw new AnServer_request_RecvCodeException($"ERROR: {response.Result.StatusCode} {Program.anServer_url}?question={question}&startindex={startindex}&count={count}", response.Result.StatusCode);
            }

            string content;
            using (var reader = new StreamReader(response.Result.Content.ReadAsStream(), Encoding.UTF8)) {
                content = reader.ReadToEnd();
            }

            try {
                return JsonSerializer.Deserialize<T>(content, Program.JSON_SERIALIZER_OPTIONS_DEFAULT);
            } catch (Exception) {
                throw new AnServer_request_DeserializationException($"ERROR: unacceptable json {content}");
            }
        }

        internal static WebStructs.Question[] AnServer_qfindRequest(string pattern, int startindex, int count) {
            var response = Program.httpClient.GetAsync($"{Program.anServer_url}?question_pattern={HttpUtility.UrlEncode(pattern)}&startindex={startindex}&count={count}");
            try {
                response.Wait();
            } catch {
                throw new AnServer_request_URLException($"URL ERROR: {Program.anServer_url}?question_pattern={pattern}&startindex={startindex}&count={count}");
            }

            if (!response.Result.IsSuccessStatusCode) {
                throw new AnServer_request_RecvCodeException($"ERROR: {response.Result.StatusCode} {Program.anServer_url}?question_pattern={pattern}&startindex={startindex}&count={count}", response.Result.StatusCode);
            }

            string content;
            using (var reader = new StreamReader(response.Result.Content.ReadAsStream(), Encoding.UTF8)) {
                content = reader.ReadToEnd();
            }

            try {
                return JsonSerializer.Deserialize<WebStructs.Question[]>(content, Program.JSON_SERIALIZER_OPTIONS_DEFAULT);
            } catch {
                throw new AnServer_request_DeserializationException($"ERROR: unacceptable json {content}");
            }
        }

        internal class AnServer_request_URLException : Exception {
            public AnServer_request_URLException(string message) : base(message) { }
        }

        internal class AnServer_request_RecvCodeException : Exception {
            public readonly System.Net.HttpStatusCode code;
            public AnServer_request_RecvCodeException(string message, System.Net.HttpStatusCode code) : base(message) {
                this.code = code;
            }
        }

        internal class AnServer_request_DeserializationException : Exception {
            public AnServer_request_DeserializationException(string message) : base(message) { }
        }
    }
}
