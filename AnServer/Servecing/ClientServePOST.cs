using System.Net;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO;


namespace AnServer {
    static partial class ClientService {

        static void ServePOST(HttpListenerContext context) {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            var headers = request.QueryString;
            if (headers.AllKeys.Any(x => x != "question")) { //Filter by "whitelist"
                response.StatusCode = 400;
                response.Close();
                return;
            }
            
            if (request.ContentType == null || !request.ContentType.ToLower().Contains("application/json")) {
                response.StatusCode = 415;
                response.Close();
                return;
            }

            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "application/json; charset=utf-8";

            string objectString;
            using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
                objectString = reader.ReadToEnd();

            string question = headers.Get("question");

            if (question == null || question.TrimEnd() == "") { //Posting question
                try {
                    var binobj = JsonSerializer.Deserialize<WebStructs.Question>(objectString, Program.JSON_SERIALIZER_OPTIONS_DEFAULT);
                    if (binobj.title.TrimEnd() == "" || binobj.title.Any(c => c == '\r' || c == '\n')) {
                        response.StatusCode = 412;
                        response.Close();
                    } else {
                        Program.dataStorageProvider.AddQuestion(binobj);
                    }
                } catch {
                    response.StatusCode = 409;
                    response.Close();
                    return;
                }
            } else {
                try {
                    var binobj = JsonSerializer.Deserialize<WebStructs.Answer>(objectString, Program.JSON_SERIALIZER_OPTIONS_DEFAULT);
                    if (binobj.body.TrimEnd() == "") {
                        response.StatusCode = 412;
                        response.Close();
                    } else {
                        Program.dataStorageProvider.AddAnswer(binobj, question);
                    }
                } catch {
                    response.StatusCode = 404;
                    response.Close();
                    return;
                }
            }

            response.StatusCode = 200;
            response.Close();
        }

    }
}
