using SchoolManager.Models;
using System.Collections.Generic;
using System.Windows;

namespace SchoolManager
{
    public partial class SignInView : Window
    {
        public User SignedInUser { get; set; }

        private List<User> _users;

        public SignInView(List<User> users)
        {
            InitializeComponent();

            _users = users;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string login = TbxLogin.Text;
            string password = TbxPassword.Password;

            foreach (User user in _users)
            {
                if (user.Login == login && user.Password == password)
                {
                    SignedInUser = user;

                    Close();

                    return;
                }
            }

            MessageBox.Show("Bledny login lub haslo");
        }
    }
}