using System;

namespace AnServer {
    public static class WebStructs {

        public class Question {
            public string title;
            public string body;
            public DateTime create_time;
        }

        public class Answer {
            public string body;
            public DateTime create_time;
        }
    }
}
