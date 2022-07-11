using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

using static System.Console;

namespace AnClient {
    static partial class Commands {
        static void ViewAnswers_requestAndDisplay(out WebStructs.Answer[] received, string question_title, int startindex, int count) {
            try {
                received = Requests.AnServer_rangebleRequest<WebStructs.Answer[]>(startindex, count, question_title);
            } catch (Exception ex) {
                WriteLine(ex.Message);
                throw;
            }

            if (received.Length != 0)
                WriteLine($"Answers {startindex}->{startindex + received.Length - 1}:");
            else
                WriteLine($"Answers {startindex}-x:");

            WriteLine(string.Concat(Enumerable.Repeat("-", 64)));
            WriteLine("{0,10} | {1,-19} | {2}", "Index", "Create time", "Preview");

            WriteLine("{0,10}|{1,-20}|{2}", string.Concat(Enumerable.Repeat("-", 11)), string.Concat(Enumerable.Repeat("-", 21)), string.Concat(Enumerable.Repeat("-", 30)));

            int i = startindex;
            foreach (var item in received) {
                WriteLine("{0,10} | {1,-19} | {2}", i++, item.create_time, Regex.Replace((item.body.Length > 32 ? item.body.Substring(0, 32) : item.body) , "[\r\n]", ""));
            }
        }

        internal static void ViewQuestion(WebStructs.Question question) {
            WebStructs.Answer[] received;

            WriteLine($"Question: {question.title}");
            WriteLine(string.Concat(Enumerable.Repeat("-", 54)));
            WriteLine($"Create time: {question.create_time}");
            WriteLine("Body:");
            WriteLine(question.body);
            WriteLine();
            try {
                ViewAnswers_requestAndDisplay(out received, question.title, 0, 10);
            } catch {
                return;
            }

            int startindex = 0, count = 10;
            
            while (true) {
                Write("\n<>->-> Enter the command: ");
                Utils.SplitCMD(ReadLine().Trim(), out List<string> parts);
                if (parts.Count == 0) continue;
                switch (parts[0]) {
                    case "help" when parts.Count == 1:
                        WriteLine("Supported commands:\n" +
                            "answer - answer the question\n" +
                            "exit - exit from the answers list\n" +
                            "next [n|all] - show next n answers or all next answers; if n not specifed, n=10\n" +
                            "prev [n|all] - show previous n answers or all previous answers; if n not specifed, n=10\n" +
                            "view {i} - view answer with index i (index i must be displayed in the table)");
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
                            ViewAnswers_requestAndDisplay(out received, question.title, startindex, count);
                        } catch {
                            return;
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
                            ViewAnswers_requestAndDisplay(out received, question.title, startindex, count);
                        } catch {
                            return;
                        }
                        continue;
                    case "view" when parts.Count == 2:
                        if (int.TryParse(parts[1], out int index) && index >= startindex && index < received.Length + startindex) {
                            var selected_answer = received[index - startindex];

                            WriteLine($"Question: {question.title}");
                            WriteLine(string.Concat(Enumerable.Repeat("-", 54)));
                            WriteLine($"Create time: {question.create_time}");
                            WriteLine("Body:");
                            WriteLine(question.body);
                            WriteLine();

                            WriteLine($"Answer (created on {selected_answer.create_time}):");
                            WriteLine(string.Concat(Enumerable.Repeat("-", 64)));
                            WriteLine("Body:");
                            WriteLine(selected_answer.body);
                            WriteLine();
                        } else {
                            WriteLine("Entered index is uncorrect. See \"help\".");
                        }
                        continue;
                    case "answer" when parts.Count == 1:
                        WriteLine($"Question: {question.title}");
                        WriteLine(string.Concat(Enumerable.Repeat("-", 54)));
                        WriteLine($"Create time: {question.create_time}");
                        WriteLine("Body:");
                        WriteLine(question.body);
                        WriteLine();

                        WriteLine("Answer (leave line empty to done):");
                        WriteLine(string.Concat(Enumerable.Repeat("-", 64)));

                        StringBuilder result_str = new();
                        var line = ReadLine();
                        while (line != "") {
                            result_str.AppendLine(line);
                            line = ReadLine();
                        }

                        if (result_str.ToString().TrimEnd() == "") {
                            WriteLine("You wrote an empty answer. Such answer cannot be posted.");
                            continue;
                        }

                        Write("Post this answer (Y/n): ");
                        var key = ReadLine();
                        if (key.Trim().ToLower() != "n") {
                            try {
                                Requests.AnServer_Post(new WebStructs.Answer() { body = result_str.ToString() }, question.title);

                                startindex = 0;
                                count = 10;
                                ViewAnswers_requestAndDisplay(out received, question.title, startindex, count);
                            } catch (Exception ex) {
                                WriteLine(ex.Message);
                            }
                        }
                        continue;
                    case "exit":
                        break;
                    default:
                        WriteLine("The command isn't recognized. Enter \"help\".");
                        continue;
                }
                break; //Exit from while if "continue" isn't called
            }
        }
    }
}
