using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Sharplog;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace DatalogExplorer.ViewModels
{
    internal class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly MainWindow view;

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Command implementations
        public static ICommand ApplicationExitCommand => new RelayCommand(
            _ =>
            {
                Application.Current.Shutdown();
            });

        public ICommand OpenFileCommand => new RelayCommand(
            p =>
            {
                var openFileDialog = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    AddExtension = true,
                    DefaultExt = "dl",
                    Filter = "Datalog|*.dl|All|*.*",
                    Multiselect = false,
                    Title = "Select datalog document to open",
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var source = File.ReadAllText(openFileDialog.FileName);
                    this.view.FactsAndRulesEditor.Text = source;
                }

            });

        public ICommand SaveFileCommand => new RelayCommand(
            p =>
            {
                var saveFileDialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    DefaultExt = "dl",
                    Filter = "Datalog|*.dl|All|*.*",
                    OverwritePrompt = true,
                    Title = "Save datalog document",
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using var reader = new System.IO.StreamReader(this.view.FactsAndRulesEditor.Text);
                    File.WriteAllText(saveFileDialog.FileName, reader.ReadToEnd());
                }
            });

        public static ICommand AboutCommand => new RelayCommand(_ =>
        {
            var aboutBox = new DatalogExplorer.Views.AboutBox();
            aboutBox.Show();

        });

        public ICommand ExecuteCommand => new RelayCommand(p =>
        {
            string src = this.view.FactsAndRulesEditor.Text;
            Stopwatch sw = new();
            sw.Start();
            var universe = new Universe();
            try
            {
                this.view.QueryEditor.Clear();
                {
                    var res = universe.ExecuteAll(src);

                    foreach (var s in res.Keys)
                    {
                        this.view.QueryEditor.Text += s.ToString() + Environment.NewLine;
                        string term = ""; 
                        foreach (var binding in res[s])
                        {
                            var first = true;
                            foreach (var nameValuePair in binding.Item2)
                            {
                                if (first)
                                {
                                    first = false;
                                }
                                else
                                {
                                    term += ", ";
                                }
                                term += nameValuePair.Key + ": " +nameValuePair.Value;

                            }
                            term += Environment.NewLine;
                            //term += ")";
                        }
                        this.view.QueryEditor.Text += term.ToString() + Environment.NewLine;
                    }
                }
                this.UpdateProgramView(universe);
            }
            catch (Exception ex)
            {
                this.view.QueryEditor.Text = ex.Message;
            }
            finally
            {
                sw.Stop();
                this.Message = $"{sw.ElapsedMilliseconds}ms elapsed";
            }
        })
        {

        };
        #endregion

        private void UpdateProgramView(Universe universe)
        {
            this.view.ProgramTree.Items.Clear();

            foreach (IGrouping<string, Sharplog.Expr> fact in universe.GetEdbProvider().AllFacts().All.GroupBy(f => f.PredicateWithArity).OrderBy(f => f.Key))
            {
                var category = new TreeViewItem() { Header = fact.Key };
                this.view.ProgramTree.Items.Add(category);

                foreach (var q in fact)
                {
                    var factNode = new TreeViewItem() { Header = q.ToString() };
                    category.Items.Add(factNode);
                }
            }

            foreach (var q in universe.Idb.GroupBy(f => f.Key).OrderBy(f => f.Key))
            {
                var category = new TreeViewItem() { Header = q.Key };
                this.view.ProgramTree.Items.Add(category);

                foreach (var r in q)
                {
                    foreach (var s in r.Value)
                    {
                        var ruleNode = new TreeViewItem() { Header = s.ToString() };
                        category.Items.Add(ruleNode);
                    }
                }
            }
        }

        public ViewModel(MainWindow w)
        {
            this.view = w;

            this.view.FactsAndRulesEditor.TextArea.Caret.PositionChanged += (object? sender, EventArgs a) =>
            {
                var caret = sender as ICSharpCode.AvalonEdit.Editing.Caret;
                this.CaretPositionString = string.Format(CultureInfo.CurrentCulture, "Line: {0} Column: {1}", caret.Line, caret.Column);
            };

            this.view.FactsAndRulesEditor.Text = @"
parent(alice, bob).
parent(alice, bart).
parent(alice, betty).

child(X,Y) :- parent(Y,X).

ancestor(X, Y) :- parent(X, Y).
ancestor(X, Y) :- ancestor(X, Z), parent(Z, Y).

sibling(X, Y) :- parent(A, X), parent(A, Y), X <> Y.

sibling(A,B)?
sibling(bob,B)?";
        }

        #region properties

        private string? _caretPositionString;
        public string? CaretPositionString
        {
            get { return this._caretPositionString;  }
            private set 
            {
                this._caretPositionString = value;
                this.OnPropertyChanged(nameof(CaretPositionString));
            }
        }

        private string? _message;
        public string? Message
        {
            get { return this._message; }
            set { this._message = value; this.OnPropertyChanged(nameof(Message));  }

        }
        int _fontSize = 12;
        public int FontSize 
        { 
            get { return _fontSize; }
            set
            {
                if (value != _fontSize)
                {
                    this._fontSize = value;
                    this.OnPropertyChanged(nameof(FontSize));
                }
            }
        }

        bool _showLineNumbers = true;
        public bool ShowLineNumbers
        {
            get { return _showLineNumbers; }
            set
            {
                if (value != _showLineNumbers)
                {
                    this._showLineNumbers = value;
                    this.OnPropertyChanged(nameof(ShowLineNumbers));
                }
            }
        }


        #endregion
    }
}
