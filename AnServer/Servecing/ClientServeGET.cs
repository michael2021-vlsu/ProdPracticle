using System;
using System.Net;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;


namespace AnServer {
    static partial class ClientService {

        static void ServeGET(HttpListenerContext context) {
            DateTime start_time = DateTime.UtcNow;
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            var headers = request.QueryString;
            if (headers.AllKeys.Any(x => x != "question" && x != "startindex" && x != "count" && x != "question_pattern")) { //Filter by "whitelist"
                response.StatusCode = 400;
                response.Close();
                return;
            }
            
            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "application/json; charset=utf-8";

            uint startindex;
            {
                string offsetString = headers.Get("startindex") ?? "0";
                if (!uint.TryParse(offsetString, out startindex) || startindex > int.MaxValue) {
                    response.StatusCode = 406;
                    response.Close();
                    return;
                }
            }

            uint count;
            {
                string countString = headers.Get("count") ?? int.MaxValue.ToString();
                if (!uint.TryParse(countString, out count) || count > int.MaxValue) {
                    response.StatusCode = 406;
                    response.Close();
                    return;
                }
            }

            List<object> responseObj;

            string question = headers.Get("question");
            if (question == null || question.TrimEnd() == "") {
                string qpattern = headers.Get("question_pattern");
                if (qpattern == null) {
                    responseObj = Program.dataStorageProvider.GetQuestions(startindex, count).ToList<object>();
                } else {
                    responseObj = Program.dataStorageProvider.FindQuestions(qpattern, startindex, count).ToList<object>();
                }
            } else {
                try {
                    responseObj = Program.dataStorageProvider.GetQuestionAnswers(question, startindex, count).ToList<object>();
                } catch {
                    response.StatusCode = 404;
                    response.Close();
                    return;
                }
            }
            
            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(responseObj, Program.JSON_SERIALIZER_OPTIONS_DEFAULT);
            response.ContentLength64 = buffer.Length;

            Stream responseStream = response.OutputStream;
            responseStream.Write(buffer, 0, buffer.Length);
            responseStream.Close();

            response.StatusCode = 200;

            response.Close();
        }

    }
}