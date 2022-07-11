using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace AnClient {
    class Utils {
        internal static void SplitCMD(string cmd, out List<string> parts) {
            parts = new List<string>();
            Stack<string> strings = new();

            //Search for decorated lines in the command and isolate them. The location points of decorated lines are saved.
            foreach (Match item in Regex.Matches(cmd, "\"[^\"]+\"").Reverse()) {
                strings.Push(item.Value.Substring(1, item.Length - 2));
                cmd = cmd.Remove(item.Index, item.Length - 1);
            }

            //Combining parts of the command and lines by the left points.
            foreach (var item in cmd.Split(' ').Where(c => c != "")) {
                if (item != "\"") {
                    parts.Add(item.ToLower()); //Case-insensetive for cmd
                } else {
                    parts.Add(strings.Pop()); //Case-sensetive for string
                }
            }
        }
    }
}
