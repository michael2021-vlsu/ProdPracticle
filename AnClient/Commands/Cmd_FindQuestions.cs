using System;
using System.Linq;
using System.Collections.Generic;

using static System.Console;

namespace AnClient {
    static partial class Commands {
        static void FindQuestions_requestAndDisplay(out WebStructs.Question[] received, string pattern, int startindex, int count) {
            try {
                received = Requests.AnServer_qfindRequest(pattern, startindex, count);
            } catch (Exception ex) {
                WriteLine(ex.Message);
                throw;
            }

            if (received.Length != 0)
                WriteLine($"Questions like \"{pattern}\", {startindex}->{startindex + received.Length - 1}:");
            else
                WriteLine($"Questions like \"{pattern}\", {startindex}->x:");

            WriteLine(string.Concat(Enumerable.Repeat("-", 54)));
            WriteLine("{0,10} | {1,-19} | {2}", "Index", "Create time", "Title");

            WriteLine("{0,10}|{1,-20}|{2}", string.Concat(Enumerable.Repeat("-", 11)), string.Concat(Enumerable.Repeat("-", 21)), string.Concat(Enumerable.Repeat("-", 20)));

            int i = startindex;
            foreach (var item in received) {
                WriteLine("{0,10} | {1,-19} | {2}", i++, item.create_time, item.title);
            }
        }

        internal static bool FindQuestions(out List<string> feedback_parts, string pattern, int count = 10) {
            feedback_parts = new();

            WebStructs.Question[] received;
            try {
                FindQuestions_requestAndDisplay(out received, pattern, 0, count);
            } catch {
                return false;
            }

            int startindex = 0;
            while (true) {
                Write("\n<>-> Enter the command: ");
                Utils.SplitCMD(ReadLine().Trim(), out List<string> parts);
                if (parts.Count == 0) continue;
                switch (parts[0]) {
                    case "help" when parts.Count == 1:
                        WriteLine("Supported commands:\n" +
                            "exit - exit from the question list\n" +
                            "find [n] {title} [n] - find questions by a title pattern (quotes are supported) and get n last of them; if n not specifed, n=10\n" +
                            "next [n|all] - show next n questions or all next questions; if n not specifed, n=10\n" +
                            "prev [n|all] - show previous n questions or all previous questions; if n not specifed, n=10\n" +
                            "view {i} - view question with index i (index i must be displayed in the table)");
                        continue;
                    case "next" when parts.Count < 3:
                        if (parts.Count == 2) {
                            if (parts[1] == "all") {
                                count = int.MaxValue;
                            } else if (ushort.TryParse(parts[1], out ushort n) && n != 0) {
                                count = n;
                            } else {
                                WriteLine("The command has uncorrect syntax. See \"help\".");
                                continue;
                            }
                        } else {
                            count = 10;
                        }
                        try {
                            checked {
                                startindex += received.Length;
                            }
                        } catch {
                            startindex = int.MaxValue;
                        }

                        try {
                            FindQuestions_requestAndDisplay(out received, pattern, startindex, count);
                        } catch {
                            return false;
                        }
                        continue;
                    case "prev" when parts.Count < 3:
                        if (parts.Count == 2) {
                            if (parts[1] == "all") {
                                count = startindex;
                                startindex = 0;
                            } else if (ushort.TryParse(parts[1], out ushort n) && n != 0) {
                                startindex -= n;
                                count = n;
                                if (startindex < 0) {
                                    count += startindex;
                                    startindex = 0;
                                }
                            } else {
                                WriteLine("The command has uncorrect syntax. See \"help\".");
                                continue;
                            }
                        } else {
                            startindex -= 10;
                            count = 10;
                            if (startindex < 0) {
                                count += startindex;
                                startindex = 0;
                            }
                        }

                        try {
                            FindQuestions_requestAndDisplay(out received, pattern, startindex, count);
                        } catch {
                            return false;
                        }
                        continue;
                    case "view" when parts.Count == 2:
                        if (int.TryParse(parts[1], out int index) && index >= startindex && index < received.Length + startindex) {

                            ViewQuestion(received[index - startindex]);

                            try {
                                FindQuestions_requestAndDisplay(out received, pattern, startindex, count);
                            } catch {
                                return false;
                            }
                            continue;
                        } else {
                            WriteLine("Entered index is uncorrect. See \"help\".");
                        }
                        continue;
                    case "find" when parts.Count < 4:
                        feedback_parts = parts;
                        return true;
                    case "exit":
                        break;
                    default:
                        WriteLine("The command isn't recognized. Enter \"help\".");
                        continue;
                }
                break; //Exit from while if "continue" isn't called
            }
            return false;
        }
    }
}
