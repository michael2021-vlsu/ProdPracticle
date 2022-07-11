using System;
using System.Net;


namespace AnServer {
    static partial class ClientService {

        public static void Serve(object contextObj) {
            HttpListenerContext context = (HttpListenerContext)contextObj;
            HttpListenerRequest request = context.Request;

            if (request.HttpMethod == "GET") {
#if DEBUG
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [INFO] New GET request");
#endif
                ServeGET(context);
            } else if (request.HttpMethod == "POST") {
#if DEBUG
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [INFO] New POST request");
#endif
                ServePOST(context);
            } else {
#if DEBUG
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [ERROR] New UNKNOWN request");
#endif
                HttpListenerResponse response = context.Response;
                response.StatusCode = 405;
                response.Close();
            }
            
        }
    }
}
