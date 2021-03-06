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
using DatalogExplorer.Properties;

namespace DatalogExplorer.ViewModels
{
    internal class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

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

        public static ICommand OpenFileCommand => new RelayCommand(
            p =>
            {
                MainWindow view = (MainWindow)p;
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
                    view.FactsAndRulesEditor.Text = source;
                }

            });

        public static ICommand SaveFileCommand => new RelayCommand(
            p =>
            {
                MainWindow view = (MainWindow)p;
                var saveFileDialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    CheckFileExists = false,
                    CheckPathExists = true,
                    DefaultExt = "dl",
                    Filter = "Datalog|*.dl|All|*.*",
                    OverwritePrompt = true,
                    Title = "Save datalog document",
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, view.FactsAndRulesEditor.Text);
                }
            });

        public static ICommand AboutCommand => new RelayCommand(_ =>
        {
            var aboutBox = new DatalogExplorer.Views.AboutBox();
            aboutBox.Show();

        });

        public static ICommand IncreaseQueryFontSizeCommand => new RelayCommand(
            target =>
            {
                var view = (MainWindow)target;
                if (Settings.Default.FontSize < 32)
                {
                    var fontSize = Settings.Default.FontSize + 2;
                    view.FactsAndRulesEditor.FontSize = fontSize;
                    view.TranscriptEditor.FontSize = fontSize;
                    Settings.Default.FontSize = fontSize;
                }
            },
            canExecute => Settings.Default.FontSize < 32);

        public static ICommand DecreaseQueryFontSizeCommand => new RelayCommand(
            target =>
            {
                var view = (MainWindow)target;
                if (Settings.Default.FontSize > 5)
                {
                    var fontSize = Settings.Default.FontSize - 2;
                    view.FactsAndRulesEditor.FontSize = fontSize;
                    view.TranscriptEditor.FontSize = fontSize;
                    Settings.Default.FontSize = fontSize;
                }
            },
            canExecute => Settings.Default.FontSize > 5);

        public ICommand ExecuteCommand => new RelayCommand(p =>
        {
            var view = (MainWindow)p;
            string src = view.FactsAndRulesEditor.Text;
            Stopwatch sw = new();
            sw.Start();
            var universe = new Universe();
            try
            {
                view.TranscriptEditor.Clear();
                {
                    var res = universe.ExecuteAll(src);

                    foreach (var s in res.Keys)
                    {
                        view.TranscriptEditor.Text += s.ToString() + Environment.NewLine;
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
                                term += nameValuePair.Key + ": " + nameValuePair.Value;

                            }
                            term += Environment.NewLine;
                        }
                        view.TranscriptEditor.Text += term.ToString() + Environment.NewLine;
                    }
                }
                UpdateProgramView(universe, view);
            }
            catch (Exception ex)
            {
                view.TranscriptEditor.Text = ex.Message;
            }
            finally
            {
                sw.Stop();
                this.Message = $"{sw.ElapsedMilliseconds}ms elapsed";
            }
        });

        #endregion

        private static void UpdateProgramView(Universe universe, MainWindow view)
        {
            view.ProgramTree.Items.Clear();

            foreach (IGrouping<string, Sharplog.Expr> fact in universe.GetEdbProvider().AllFacts().All.GroupBy(f => f.PredicateWithArity).OrderBy(f => f.Key))
            {
                var category = new TreeViewItem() { Header = fact.Key };
                view.ProgramTree.Items.Add(category);

                foreach (var q in fact)
                {
                    var factNode = new TreeViewItem() { Header = q.ToString() };
                    category.Items.Add(factNode);
                }
            }

            foreach (var q in universe.Idb.GroupBy(f => f.Key).OrderBy(f => f.Key))
            {
                var category = new TreeViewItem() { Header = q.Key };
                view.ProgramTree.Items.Add(category);

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

        public ViewModel()
        {
            Properties.Settings.Default.PropertyChanged += (object? sender, PropertyChangedEventArgs e) =>
            {
                // Save all the user's settings when they change.
                Properties.Settings.Default.Save();
            };
        }
        #region properties

        private string? _caretPositionString;
        public string? CaretPositionString
        {
            get { return this._caretPositionString;  }
            set 
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
