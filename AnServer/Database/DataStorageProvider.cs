using System;
using System.Threading;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AnServer {
    internal class DataStorageProvider : IDisposable {
        LocalDB db;
        ReaderWriterLockSlim lockDb = new();

        internal DataStorageProvider(string file_path) {
            DbContextOptionsBuilder opts = new();
            opts.UseSqlite("Data Source=\"" + file_path + "\"",
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            db = new(opts.Options);

            db.Database.EnsureCreated();
        }

        public void Dispose() {
            db.Database.CloseConnection();
            db.Dispose();
            lockDb.Dispose();
        }

        internal void AddQuestion(WebStructs.Question question) {
            lockDb.EnterWriteLock();
            db.table_questions.Add(new() {
                title = question.title,
                body = question.body,
                create_time = DateTime.UtcNow
            });
            try {
                db.SaveChanges();
            } catch (DbUpdateException ex) {
                throw new DbProviderUniqueKeyException(ex);
            } finally {
                lockDb.ExitWriteLock();
            }
        }

        internal void AddAnswer(WebStructs.Answer answer, string question_title) {
            lockDb.EnterUpgradeableReadLock();

            try {
                var question_ref = db.table_questions.Single(x => x.title == question_title);

                lockDb.EnterWriteLock();
                db.table_answers.Add(new() {
                    body = answer.body,
                    create_time = DateTime.UtcNow,
                    question = question_ref
                });
                db.SaveChanges();
                lockDb.ExitWriteLock();
            } catch (InvalidOperationException ex) {
                throw new DbProviderNotFound(ex, "Question with specifed \"question_title\" isn't found!");
            } finally {
                lockDb.ExitUpgradeableReadLock();
            }
        }

        internal IEnumerable<WebStructs.Question> GetQuestions(uint start_id = 0, uint count = int.MaxValue) {
            lockDb.EnterReadLock();
            var questioins = db.table_questions
                    .OrderByDescending(p => p.create_time)
                    .Skip((int)start_id)
                    .Take((int)count)
                    .Select(x => new WebStructs.Question() { title = x.title, body = x.body, create_time = x.create_time });
            lockDb.ExitReadLock();
            return questioins;
        }

        internal IEnumerable<WebStructs.Question> FindQuestions(string text, uint start_id = 0, uint count = 10) {
            lockDb.EnterReadLock();
            var questioins = db.table_questions
                .Where(p => EF.Functions.Like(p.title, text))
                .OrderByDescending(p => p.create_time)
                .Skip((int)start_id)
                .Take((int)count)
                .Select(x => new WebStructs.Question() { title = x.title, body = x.body, create_time = x.create_time });
            lockDb.ExitReadLock();
            return questioins;
        }

        internal IEnumerable<WebStructs.Answer> GetQuestionAnswers(string question_title, uint start_id = 0, uint count = int.MaxValue) {
            lockDb.EnterReadLock();
            try {
                return db.table_questions
                    .Include(p => p.answers)
                    .Single(q => q.title == question_title)
                    .answers
                    .OrderByDescending(p => p.create_time)
                    .Skip((int)start_id)
                    .Take((int)count)
                    .Select(x => new WebStructs.Answer() { body = x.body, create_time = x.create_time });
            } catch (InvalidOperationException ex) {
                throw new DbProviderNotFound(ex, "Question with specifed \"question_title\" isn't found!");
            } finally {
                lockDb.ExitReadLock();
            }
        }

        class LocalDB : DbContext {
            internal LocalDB(DbContextOptions options) : base(options) { }

            public DbSet<DbStructs.dbQuestion> table_questions { get; set; }
            public DbSet<DbStructs.dbAnswer> table_answers { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                modelBuilder.Entity<DbStructs.dbQuestion>(entity => {
                    entity.HasIndex(p => p.create_time);

                    entity.HasIndex(p => p.title).IsUnique();
                });

                modelBuilder.Entity<DbStructs.dbAnswer>(entity => {
                    entity.HasIndex(p => p.create_time);

                    entity.HasOne(e => e.question)
                    .WithMany(e => e.answers)
                    .HasForeignKey(e => e.question_id)
                    .OnDelete(DeleteBehavior.Cascade);
                });
            }
        }

        class DbProviderUniqueKeyException : Exception {
            public DbProviderUniqueKeyException(Exception inner) : base("The \"title\" feild is not unique!", inner) { }
        }

        class DbProviderNotFound : Exception {
            public DbProviderNotFound(Exception inner, string message) : base(message, inner) { }
        }
    }
}
