using Microsoft.EntityFrameworkCore;
using SchoolManager.Enums;
using SchoolManager.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SchoolManager
{
    public partial class MainWindow : Window
    {
        private SchoolManagerContext _schoolManagerContext = new SchoolManagerContext();

        private User _signedInUser;

        public MainWindow()
        {
            InitializeComponent();

            //PopulateData();
            //return;

            List<Teacher> allTeachers = _schoolManagerContext.Teachers.ToList();
            List<Parent> allParents = _schoolManagerContext.Parents.ToList();
            List<Student> allStudents = _schoolManagerContext.Students.ToList();

            List<User> users = new List<User>();

            foreach (Teacher teacher in allTeachers)
            {
                teacher.Type = UserTypes.Teacher;
                users.Add(teacher);
            }

            foreach (Parent parent in allParents)
            {
                parent.Type = UserTypes.Parent;
                users.Add(parent);
            }

            foreach (Student student in allStudents)
            {
                student.Type = UserTypes.Student;
                users.Add(student);
            }

            SignInView signInView = new SignInView(users);

            signInView.ShowDialog();

            _signedInUser = signInView.SignedInUser;

            switch (_signedInUser.Type)
            {
                case UserTypes.Parent:

                    InitializeParentTab();

                    break;

                case UserTypes.Student:

                    InitializeStudentTab();

                    break;

                case UserTypes.Teacher:

                    InitializeTeacherTab();

                    break;
            }
        }

        private void InitializeParentTab()
        {
            TabTeacher.Visibility = Visibility.Collapsed;
            TabStudent.Visibility = Visibility.Collapsed;

            List<Student> students = _schoolManagerContext.Students
                .Include(s => s.Subjects)
                .ThenInclude(s => s.Grades)
                .ToList();

            foreach (Student student in students)
            {
                if (student.Parent == _signedInUser)
                {
                    DGParentChildren.Items.Insert(DGParentChildren.Items.Count, student);
                }
            }
        }

        private void InitializeStudentTab()
        {
            TabParent.Visibility = Visibility.Collapsed;
            TabTeacher.Visibility = Visibility.Collapsed;

            List<Student> students = _schoolManagerContext.Students
                .Include(s => s.Subjects)
                .ThenInclude(s => s.Grades)
                .ToList();

            Student signedInStuddent = _schoolManagerContext.Students
                .Include(s => s.Subjects)
                .ThenInclude(s => s.Grades)
                .SingleOrDefault(s => s.Name == _signedInUser.Name && s.Surname == _signedInUser.Surname);

            foreach (Subject subject in signedInStuddent.Subjects)
            {
                string gradesString = GetGradesString(subject.Grades.ToList());

                decimal average = (decimal)subject.Grades.Sum(g => g.Value) / subject.Grades.Count();

                int position = CalculateSubjectPositionInClass(students, signedInStuddent, subject.Name);

                var studentGrades = new { Subject = subject.Name, Grades = gradesString, Average = average, Position = position };

                DGStudentGrades.Items.Insert(DGStudentGrades.Items.Count, studentGrades);
            }
        }

        private void InitializeTeacherTab()
        {
            TabParent.Visibility = Visibility.Collapsed;
            TabStudent.Visibility = Visibility.Collapsed;
        }

        private void DGParentChildren_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DGParentChildrenGrades.Items.Clear();

            Student selectedStudent = (Student)DGParentChildren.SelectedItem;

            foreach (Subject subject in selectedStudent.Subjects)
            {
                string gradesString = GetGradesString(subject.Grades.ToList());

                DGParentChildrenGrades.Items.Insert(DGParentChildrenGrades.Items.Count, new { Subject = subject.Name, Grades = gradesString });
            }
        }

        private string GetGradesString(List<Grade> grades)
        {
            string gradesString = string.Empty;

            foreach (Grade grade in grades)
            {
                gradesString += $"{grade.Value} ";
            }

            return gradesString;
        }

        private int CalculateSubjectPositionInClass(List<Student> students, Student student, string subject)
        {
            Dictionary<Student, decimal> positions = new Dictionary<Student, decimal>();

            foreach (Student checkingStudent in students)
            {
                Subject averageSubject = checkingStudent.Subjects.SingleOrDefault(s => s.Name == subject); // LINQ

                if (averageSubject == null)
                {
                    positions.Add(checkingStudent, 0);
                }

                decimal average = (decimal)averageSubject.Grades.Sum(g => g.Value) / averageSubject.Grades.Count();

                positions.Add(checkingStudent, average);
            }



            return 0;
        }

        // Do usunięcia
        private void PopulateData()
        {
            //var x = _schoolManagerContext.Students.Include(s => s.Subjects).ToList();
            //x.ToList()[0].Subjects = x.ToList()[0].Subjects.Distinct().ToList();
            //var p = _schoolManagerContext.Parents.First(p => p.Name == "Parent");

            //var ss = new Student { Name = "Student1", Surname = "Nowak", Login = "ss", Password = "ss", Type = UserTypes.Student, Parent = p };
            //_schoolManagerContext.Students.Add(ss);
            //_schoolManagerContext.SaveChanges();

            //var s = _schoolManagerContext.Students.First(s => s.Name == "Student");
            //var ss = _schoolManagerContext.Students.First(s => s.Name == "Student1");

            //var x = new Subject { Name = "Matematyka", Student = s };
            //x.Grades.Add(new Grade { Value = 1 });
            //x.Grades.Add(new Grade { Value = 3 });
            //ss.Subjects.Add(x);

            //var xx = new Subject { Name = "Matematyka", Student = ss };
            //xx.Grades.Add(new Grade { Value = 4 });
            //xx.Grades.Add(new Grade { Value = 6 });
            //s.Subjects.Add(xx);

            //var y = new Subject { Name = "Polski", Student = s };
            //y.Grades.Add(new Grade { Value = 3 });
            //y.Grades.Add(new Grade { Value = 2 });
            //y.Grades.Add(new Grade { Value = 5 });
            //ss.Subjects.Add(y);

            //var yy = new Subject { Name = "Polski", Student = ss };
            //yy.Grades.Add(new Grade { Value = 2 });
            //yy.Grades.Add(new Grade { Value = 4 });
            //yy.Grades.Add(new Grade { Value = 3 });
            //yy.Grades.Add(new Grade { Value = 2 });
            //yy.Grades.Add(new Grade { Value = 4 });
            //s.Subjects.Add(yy);

            //var z = new Subject { Name = "Chemia", Student = s };
            //z.Grades.Add(new Grade { Value = 1 });
            //z.Grades.Add(new Grade { Value = 3 });
            //z.Grades.Add(new Grade { Value = 2 });
            //ss.Subjects.Add(z);

            //var zz = new Subject { Name = "Chemia", Student = ss };
            //zz.Grades.Add(new Grade { Value = 5 });
            //zz.Grades.Add(new Grade { Value = 6 });
            //zz.Grades.Add(new Grade { Value = 5 });
            //zz.Grades.Add(new Grade { Value = 5 });
            //s.Subjects.Add(zz);


            //_schoolManagerContext.SaveChanges();
        }
    }
}