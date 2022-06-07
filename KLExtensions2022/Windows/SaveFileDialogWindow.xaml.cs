using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace KLExtensions2022
{
    public partial class SaveFileDialogWindow : BaseDialogWindow
    {
        private const string DEFAULT_TEXT = "Enter a file name";
        public string Input => txtName.Text.Trim();
        public Func<string, Task> ActionToDo { get; set; } = null;
        public Action ActionToClose { get; set; } = null;
        public string NameSpaceType { get; set; }

        public SaveFileDialogWindow(string labelName, string fileName, Func<string, Task> actionToDo)
        {
            InitializeComponent();
            this.lblText.Content = labelName;
            this.txtName.Text = fileName;
            this.ActionToDo = actionToDo;
            GetNameSpaceType();
        }

        public SaveFileDialogWindow(string labelName, string fileName)
        {
            InitializeComponent();
            this.lblText.Content = labelName;
            this.txtName.Text = fileName;
            GetNameSpaceType();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            if (ActionToClose == null)
            {
                Close();
            }
            else
            {
                ActionToClose?.Invoke();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtName.Text))
            {
                DialogResult = true;
                GetNameSpaceType();
                if (ActionToClose == null)
                {
                    Close();
                }
                else if (ActionToDo != null)
                {
                    ActionToClose?.Invoke();
                    ActionToDo?.Invoke(txtName.Text);
                }
            }
        }

        private void GetNameSpaceType()
        {
            if (btnProject.IsChecked == true)
            {
                this.NameSpaceType = "Project";
            }
            else
            {
                this.NameSpaceType = "Folder";
            }
        }
    }
}
