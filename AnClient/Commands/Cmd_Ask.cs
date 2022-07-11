using System;
using System.Text;
using System.Net;
using System.Linq;

using static System.Console;

namespace AnClient {
    static partial class Commands {
        internal static bool Ask() {
            WriteLine($"Question:");
            WriteLine(string.Concat(Enumerable.Repeat("-", 54)));
            Write("Title (leave empty to exit): ");

            var title = ReadLine();
            if (title.TrimEnd() == "") {
                return false;
            }

            WriteLine("Body (leave line empty to done):");
            
            StringBuilder result_str = new();
            var line = ReadLine();
            while (line != "") {
                result_str.AppendLine(line);
                line = ReadLine();
            }

            Write("Post this question (Y/n): ");
            var key = ReadLine();
            if (key.Trim().ToLower() != "n") {
                try {
                    Requests.AnServer_Post(new WebStructs.Question() { title = title, body = result_str.ToString() });
                    return true;
                } catch (Requests.AnServer_request_RecvCodeException ecode) {
                    if (ecode.code == HttpStatusCode.Conflict) {
                        WriteLine("A question with exactly such title has already been asked. Maybe there is an answer to it that you need?");
                    }
                } catch (Exception ex) {
                    WriteLine(ex.Message);
                }
            }
            return false;
        }
    }
}
