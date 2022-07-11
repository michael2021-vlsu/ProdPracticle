using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;


namespace AnServer {
    class Program {
        internal const string ADDRESS_URL = "http://127.0.0.1:8080/anserver/";

        internal static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS_DEFAULT = new(JsonSerializerDefaults.Web) {
            IncludeFields = true
        };

        internal static DataStorageProvider dataStorageProvider;

        static void Main() {
            if (!HttpListener.IsSupported) {
                Console.WriteLine("[ERROR] HttpListener is not supported!");
                return;
            }

            dataStorageProvider = new("anServer_storage.sqlite");

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(ADDRESS_URL);
            listener.Start();

#if DEBUG
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [INFO] Server started");
            Console.WriteLine("[INFO] URL: " + ADDRESS_URL);
#endif
            while (true) {
                HttpListenerContext context = listener.GetContext();
                Task.Factory.StartNew(ClientService.Serve, context);
            }
        }

    }
}
