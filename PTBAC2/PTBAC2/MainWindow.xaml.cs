using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PTBAC2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string currentText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            Regex regex = new Regex(@"^-?\d*\.?\d*$");
            e.Handled = !regex.IsMatch(currentText);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            aTextBox.Clear();
            bTextBox.Clear();
            cTextBox.Clear();
            resultTextBox.Clear();
            x1TextBox.Clear();
            x2TextBox.Clear();
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double a = double.Parse(aTextBox.Text);
                double b = double.Parse(bTextBox.Text);
                double c = double.Parse(cTextBox.Text);


                double delta = b * b - 4 * a * c;

                if (delta < 0)
                {
                    resultTextBox.Text = "No solutions.";
                    x1TextBox.Clear();
                    x2TextBox.Clear();
                }
                else if (delta == 0)
                {
                    double x = -b / (2 * a);
                    resultTextBox.Text = "1 solution";
                    x1TextBox.Text = x.ToString();
                    x2TextBox.Text = x.ToString();
                }
                else
                {
                    double sqrtDelta = Math.Sqrt(delta);
                    double x1 = (-b + sqrtDelta) / (2 * a);
                    double x2 = (-b - sqrtDelta) / (2 * a);

                    resultTextBox.Text = "2 solutions ";
                    x1TextBox.Text = x1.ToString();
                    x2TextBox.Text = x2.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please enter valid numbers for a, b, and c.");
            }
        }
    }
}
