using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnServer {
    public static class DbStructs {

        [Table("questions")]
        public class dbQuestion {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public uint id { get; set; }

            [Required]
            public string title { get; set; }

            [Required]
            public string body { get; set; }

            [Required]
            public DateTime create_time { get; set; }

            public virtual ICollection<dbAnswer> answers { get; set; }
        }

        [Table("answers")]
        public class dbAnswer {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public uint id { get; set; }

            [Required]
            public string body { get; set; }

            [Required]
            public DateTime create_time { get; set; }

            [Required]
            public uint question_id { get; set; }
            public virtual dbQuestion question { get; set; }
        }
    }
}
