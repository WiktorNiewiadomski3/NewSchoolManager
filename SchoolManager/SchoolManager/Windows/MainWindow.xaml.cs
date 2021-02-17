using Microsoft.EntityFrameworkCore;
using SchoolManager.Enums;
using SchoolManager.Models;
using System;
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

            _schoolManagerContext.EnsureDatabaseIsPopulatedWithData();

            SignInView signInView = new SignInView(_schoolManagerContext.Users);

            signInView.ShowDialog();

            if (signInView.SignedInUser == null)
            {
                Environment.Exit(0);
            }

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
            PnlTabs.SelectedItem = TabParent;
            TabParent.Header = $"Rodzic - {_signedInUser.Name} {_signedInUser.Surname}";

            List<Student> students = _schoolManagerContext.Students
                .Include(s => s.Subjects)
                .ThenInclude(s => s.Grades)
                .ToList();

            foreach (Student student in students)
            {
                bool studentIsSignedInParentChild = student.Parent == _signedInUser;

                if (studentIsSignedInParentChild)
                {
                    DGParentChildren.Items.Insert(DGParentChildren.Items.Count, student);
                }
            }
        }

        private void InitializeStudentTab()
        {
            TabParent.Visibility = Visibility.Collapsed;
            TabTeacher.Visibility = Visibility.Collapsed;
            PnlTabs.SelectedItem = TabStudent;
            TabStudent.Header = $"Uczeń - {_signedInUser.Name} {_signedInUser.Surname}";

            List<Student> students = _schoolManagerContext.Students
                .Include(s => s.Subjects)
                .ThenInclude(s => s.Grades)
                .ToList();

            Student signedInStuddent = _schoolManagerContext.Students
                .Include(s => s.Subjects)
                .ThenInclude(s => s.Grades)
                .SingleOrDefault(s => s.Id == _signedInUser.Id);

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
            PnlTabs.SelectedItem = TabTeacher;
            TabTeacher.Header = $"Nauczyciel - {_signedInUser.Name} {_signedInUser.Surname}";

            DGTeacherStudents.Items.Clear();
            DGTeacherStudentsGrades.Items.Clear();

            List<Student> students = _schoolManagerContext.Students
                .Include(s => s.Class)
                .Include(s => s.Subjects)
                .ThenInclude(s => s.Grades)
                .ToList();

            Teacher signedInTeacher = _schoolManagerContext.Teachers
                .Include(t => t.Classes)
                .SingleOrDefault(t => t.Id == _signedInUser.Id);

            Class teacherClass = signedInTeacher.Classes.First();

            LblTeacherStudents.Content += $" {teacherClass.Name}";

            foreach (Student student in students)
            {
                bool studentBelongsToSignedInTeacherClass = student.Class.Teacher == _signedInUser;

                if (studentBelongsToSignedInTeacherClass)
                {
                    DGTeacherStudents.Items.Insert(DGTeacherStudents.Items.Count, student);
                }
            }
        }

        private void DGParentChildren_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PopulateGradesGridBySelectedStudent(DGParentChildren, DGParentChildrenGrades);
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
                Subject averageSubject = checkingStudent.Subjects.SingleOrDefault(s => s.Name == subject);

                if (averageSubject == null)
                {
                    positions.Add(checkingStudent, 0);
                    continue;
                }

                decimal average = (decimal)averageSubject.Grades.Sum(g => g.Value) / averageSubject.Grades.Count();

                positions.Add(checkingStudent, average);
            }

            var orderedPositions = positions.OrderBy(x => x.Value).Reverse().ToList();

            int position = 1;

            for (int i = 0; i < orderedPositions.Count - 1; i++)
            {
                Student currentStudent = orderedPositions[i].Key;

                if (currentStudent == student)
                {
                    break;
                }

                position++;
            }

            return position;
        }

        private void DGTeacherStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PopulateGradesGridBySelectedStudent(DGTeacherStudents, DGTeacherStudentsGrades);
        }

        private void PopulateGradesGridBySelectedStudent(DataGrid studentsGrid, DataGrid gradesGrid)
        {
            gradesGrid.Items.Clear();

            Student selectedStudent = (Student)studentsGrid.SelectedItem;

            bool gridIsBeingCleared = selectedStudent == null;

            if (selectedStudent == null)
            {
                return;
            }

            foreach (Subject subject in selectedStudent.Subjects)
            {
                string gradesString = GetGradesString(subject.Grades.ToList());

                gradesGrid.Items.Insert(gradesGrid.Items.Count, new { Subject = subject.Name, Grades = gradesString });
            }
        }

        private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TbxStudentName.Text))
            {
                MessageBox.Show("Musisz podać imię dla nowego studenta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TbxStudentSurname.Text))
            {
                MessageBox.Show("Musisz podać nazwisko dla nowego studenta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TbxStudentLogin.Text))
            {
                MessageBox.Show("Musisz podać login dla nowego studenta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TbxStudentPassword.Text))
            {
                MessageBox.Show("Musisz podać hasło dla nowego studenta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool studentWithTheSameLoginAlreadyExists = _schoolManagerContext.Students.Any(s => s.Login == TbxStudentLogin.Text);

            if (studentWithTheSameLoginAlreadyExists)
            {
                MessageBox.Show("Istnieje już student o podanym loginie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Student newStudent = new Student
            {
                Name = TbxStudentName.Text,
                Surname = TbxStudentSurname.Text,
                Login = TbxStudentLogin.Text,
                Password = TbxStudentPassword.Text
            };

            Teacher signedInTeacher = _schoolManagerContext.Teachers
                .Include(t => t.Classes)
                .SingleOrDefault(t => t.Id == _signedInUser.Id);

            Class teacherClass = signedInTeacher.Classes.First();

            teacherClass.Students.Add(newStudent);

            _schoolManagerContext.SaveChanges();

            TbxStudentName.Clear();
            TbxStudentSurname.Clear();
            TbxStudentLogin.Clear();
            TbxStudentPassword.Clear();

            InitializeTeacherTab();
        }

        private void BtnRemoveStudent_Click(object sender, RoutedEventArgs e)
        {
            if (DGTeacherStudents.SelectedItem == null)
            {
                MessageBox.Show("Musisz wybrać studenta z listy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Student student = (Student)DGTeacherStudents.SelectedItem;

            _schoolManagerContext.Remove(student);
            _schoolManagerContext.SaveChanges();

            InitializeTeacherTab();
        }

        private void BtnAddGrade_Click(object sender, RoutedEventArgs e)
        {
            if (DGTeacherStudents.SelectedItem == null)
            {
                MessageBox.Show("Musisz wybrać studenta z listy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CbxSubject.SelectedItem == null)
            {
                MessageBox.Show("Musisz wybrać przedmiot.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CbxGrade.SelectedItem == null)
            {
                MessageBox.Show("Musisz wybrać ocenę.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ComboBoxItem selectedSubjectItem = (ComboBoxItem)CbxSubject.SelectedItem;
            string subjectName = selectedSubjectItem.Content.ToString();

            ComboBoxItem selectedGradeItem = (ComboBoxItem)CbxGrade.SelectedItem;
            int gradeValue = Convert.ToInt32(selectedGradeItem.Content);

            Student student = (Student)DGTeacherStudents.SelectedItem;

            Subject subject = student.Subjects.Single(s => s.Name == subjectName);

            Grade newGrade = new Grade { Value = gradeValue };

            subject.Grades.Add(newGrade);

            _schoolManagerContext.SaveChanges();

            PopulateGradesGridBySelectedStudent(DGTeacherStudents, DGTeacherStudentsGrades);
        }
    }
}