using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace DatalogExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModels.ViewModel viewModel;
        public MainWindow()
        {
            this.InitializeComponent();
            var viewmodel = new ViewModels.ViewModel();
            this.viewModel = viewmodel;

            this.FactsAndRulesEditor.TextArea.Caret.PositionChanged += (object? sender, EventArgs a) =>
            {
                var caret = (ICSharpCode.AvalonEdit.Editing.Caret)sender;
                viewmodel.CaretPositionString = string.Format(CultureInfo.CurrentCulture, "Line: {0} Column: {1}", caret.Line, caret.Column);
            };
            this.DataContext = viewModel;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);


            this.FactsAndRulesEditor.Text = @"
parent(alice, bob).
parent(alice, bart).
parent(alice, betty).

child(X,Y) :- parent(Y, X).

ancestor(X, Y) :- parent(X, Y).
ancestor(X, Y) :- ancestor(X, Z), parent(Z, Y).

sibling(X, Y) :- parent(A, X), parent(A, Y), X <> Y.

sibling(A,B)?
sibling(bob,B)?";

        }

        protected virtual void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            if (sender is ICSharpCode.AvalonEdit.TextEditor editor)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if (e.Delta > 0)
                    {
                        this.IncreaseQueryFontSizeButton.Command.Execute(this);
                    }
                    else
                    {
                        this.DecreaseQueryFontSizeButton.Command.Execute(this);
                    }
                }
            }
        }
    }
}
