using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSQLiteDemo
{
    // Entity Models
    public class Student
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public List<Subject> Subjects { get; set; } = new();
    }

    public class Subject
    {
        public int SubjectId { get; set; }
        public string Title { get; set; }
        public int MaximumCapacity { get; set; }
        public List<Student> Students { get; set; } = new();
    }

    // Database Context
    public class SchoolContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=School.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure many-to-many relationship without explicit table name
            modelBuilder.Entity<Student>()
                .HasMany(s => s.Subjects)
                .WithMany(s => s.Students);
        }
    }

    // Repository Class
    public class SchoolRepository
    {
        private readonly SchoolContext _context;

        public SchoolRepository(SchoolContext context)
        {
            _context = context;
        }

        public void AddSubject(Subject subject)
        {
            _context.Subjects.Add(subject);
            _context.SaveChanges();
        }

        public void AddStudent(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();
        }

        public void EnrollStudentToSubject(int studentId, int subjectId)
        {
            var student = _context.Students.Find(studentId);
            var subject = _context.Subjects.Find(subjectId);

            if (student != null && subject != null)
            {
                student.Subjects.Add(subject);
                _context.SaveChanges();
            }
        }

        public List<Subject> GetAllSubjects()
        {
            return _context.Subjects.Include(s => s.Students).ToList();
        }

        public List<Student> GetStudentsForSubject(int subjectId)
        {
            return _context.Subjects
                .Include(s => s.Students)
                .FirstOrDefault(s => s.SubjectId == subjectId)?.Students;
        }
    }

    // Main Program
    class Program
    {
        static void Main(string[] args)
        {
            using var context = new SchoolContext();
            context.Database.EnsureDeleted(); // Clean slate for demo
            context.Database.EnsureCreated();

            var repository = new SchoolRepository(context);

            // Add a subject
            var math = new Subject { Title = "Mathematics", MaximumCapacity = 30 };
            repository.AddSubject(math);

            // Add students
            var student1 = new Student { Name = "Alice", EnrollmentDate = DateTime.Now };
            var student2 = new Student { Name = "Bob", EnrollmentDate = DateTime.Now };

            repository.AddStudent(student1);
            repository.AddStudent(student2);

            // Enroll students to the subject
            repository.EnrollStudentToSubject(student1.StudentId, math.SubjectId);
            repository.EnrollStudentToSubject(student2.StudentId, math.SubjectId);

            // Fetch and display data
            var subjects = repository.GetAllSubjects();
            foreach (var subject in subjects)
            {
                Console.WriteLine($"Subject: {subject.Title}");
                foreach (var student in subject.Students)
                {
                    Console.WriteLine($" - {student.Name}");
                }
            }
        }
    }
}
