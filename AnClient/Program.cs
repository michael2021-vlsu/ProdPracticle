using System;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;

using static System.Console;

namespace AnClient {
    class Program {
        internal static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS_DEFAULT = new(JsonSerializerDefaults.Web) {
            IncludeFields = true
        };

        internal static string anServer_url;

        internal static HttpClient httpClient = new() {
            Timeout = TimeSpan.FromSeconds(10)
        };


        static void Main() {
            WriteLine("==== AnClient ====");

            Write("AnServer address is: \"http://localhost:8080/anserver\"? (Y/n) ");

            {
                var key = ReadLine();
                if (key.Trim().ToLower() != "n") {
                    anServer_url = "http://localhost:8080/anserver";
                } else {
                    do {
                        WriteLine("Enter the AnServer url (example: http://localhost:8080/anserver)");
                        Write(">");
                        anServer_url = ReadLine();
                    } while (!Uri.TryCreate(anServer_url, UriKind.Absolute, out Uri result) || result.Scheme != Uri.UriSchemeHttp);
                }
            }

            List<string> parts;
            do {
                Write("\n<> Enter the command: ");
                Utils.SplitCMD(ReadLine().Trim(), out parts);
                if (parts.Count != 0) {
                    switch (parts[0]) {
                        case "ask" when parts.Count == 1:
                            if (Commands.Ask())
                                Commands.ListQuestions();
                            break;
                        case "list" when parts.Count < 3:
                            if (parts.Count == 2) {
                                if (ushort.TryParse(parts[1], out ushort n) && n != 0) {
                                    Commands.ListQuestions(n);
                                } else {
                                    WriteLine("The command has uncorrect syntax. See \"help\".");
                                    continue;
                                }
                            } else {
                                Commands.ListQuestions();
                            }
                            break;
                        case "find" when parts.Count < 4:
                            while (true) {
                                switch (parts.Count) {
                                    case 2:
                                        if (Commands.FindQuestions(out parts, parts[1]))
                                            continue;
                                        break;
                                    case 3:
                                        ushort n;
                                        if (ushort.TryParse(parts[1], out n) && n != 0) {
                                            if (Commands.FindQuestions(out parts, parts[2], n))
                                                continue;
                                        } else if (ushort.TryParse(parts[2], out n) && n != 0) {
                                            if (Commands.FindQuestions(out parts, parts[1], n))
                                                continue;
                                        } else {
                                            WriteLine("The command has uncorrect syntax. See \"help\".");
                                        }
                                        break;
                                    default:
                                        WriteLine("The command has uncorrect syntax. See \"help\".");
                                        break;
                                }
                                break;
                            }
                            break;
                        case "help" when parts.Count == 1:
                            WriteLine("Supported commands:\n" +
                            "ask - write a question\n" +
                            "exit - exit from the program\n" +
                            "find [n] {title} [n] - find questions by a title pattern (quotes are supported) and get n last of them; if n not specifed, n=10\n" +
                            "list [n] - show previous n answers or all previous answers; if n not specifed, n=10\n");
                            break;
                        case "exit":
                            break;
                        default:
                            WriteLine("The command isn't recognized. Enter \"help\".");
                            break;
                    }
                }
            } while (parts.Count == 0 || parts[0] != "exit");

            return;
        }
    }
}
