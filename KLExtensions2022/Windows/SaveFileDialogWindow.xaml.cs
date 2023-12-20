using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;

namespace KLExtensions2022
{
    public partial class SaveFileDialogWindow : BaseDialogWindow
    {
        private const string DEFAULT_TEXT = "Enter a file name";
        public string Input => txtName.Text.Trim();
        public Func<string, Task> ActionToDo { get; set; } = null;
        public Action ActionToClose { get; set; } = null;
        public NamespaceOptions NamespaceOptions { get; set; }
        public bool UseImplicitUsings { get; set; }
        public bool IsStatic { get; set; } = false;
        public bool IsRoslyn { get; set; } = false;

        public SaveFileDialogWindow (string labelName, string fileName, Func<string, Task> actionToDo)
        {
            InitializeComponent();
            this.lblText.Content = labelName;
            this.txtName.Text = fileName;
            this.ActionToDo = actionToDo;
            GetNameSpaceType();
            GetUsingsType();
            txtName.Focus();
            txtName.SelectAll();
        }

        public SaveFileDialogWindow (string labelName, string fileName)
        {
            InitializeComponent();
            this.lblText.Content = labelName;
            this.txtName.Text = fileName;
            GetNameSpaceType();
            GetUsingsType();
            txtName.Focus();
            txtName.SelectAll();
        }

        public SaveFileDialogWindow(string labelName, string fileName, NamespaceOptions namespaceOptions, bool useImplicitUsings)
        {
            InitializeComponent();
            this.lblText.Content = labelName;
            this.txtName.Text = fileName;
            this.NamespaceOptions = namespaceOptions;
            this.UseImplicitUsings = useImplicitUsings;
            GetNameSpaceType();
            GetUsingsType();
            txtName.Focus();
            txtName.SelectAll();
        }

        private void Button_Click (object sender, RoutedEventArgs e)
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

        private void Button_Click_1 (object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtName.Text))
            {
                DialogResult = true;

                if (ActionToClose == null)
                {
                    Close();
                }
                else if (ActionToDo != null)
                {
                    ActionToClose?.Invoke();
                    _ = ActionToDo.Invoke(txtName.Text);
                }
            }
        }

        private void GetNameSpaceType()
        {
            if (this.NamespaceOptions == NamespaceOptions.Project)
            {
                this.btnProject.IsChecked = true;
            }
            else
            {
                this.btnFolder.IsChecked = true;
            }
        }

        private void GetUsingsType()
        {
            if (this.UseImplicitUsings == true)
            {
                this.btnUsingsTrue.IsChecked = true;
            }
            else
            {
                this.btnUsingsFalse.IsChecked = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                DialogResult = true;

                if (ActionToClose == null)
                {
                    Close();
                }
                else if (ActionToDo != null)
                {
                    ActionToClose?.Invoke();
                    _ = ActionToDo.Invoke(txtName.Text);
                }
            }
        }

        private void chkStatic_CheckedChanged(object sender, RoutedEventArgs e)
        {
            this.IsStatic = (chkStatic.IsChecked == true) ? true : false;
        }

        private void chkRoslyn_CheckedChanged(object sender, RoutedEventArgs e)
        {
            this.IsRoslyn = (chkRoslyn.IsChecked == true) ? true: false;
        }

        private void NamespaceProject_CheckChanged(object sender, RoutedEventArgs e)
        {
            NamespaceOptions = NamespaceOptions.Project;
        }

        private void NamespaceFolder_CheckChanged(object sender, RoutedEventArgs e)
        {
            NamespaceOptions = NamespaceOptions.Folder;
        }

        private void UsingsTrue_CheckChanged(object sender, RoutedEventArgs e)
        {
            UseImplicitUsings = true;
        }

        private void UsingsFalse_CheckChanged(object sender, RoutedEventArgs e)
        {
            UseImplicitUsings = false;
        }
    }
}
