using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Compiler
{
    internal abstract class Repl
    {
        private readonly List<string> _history = new List<string>();
        private int _historyIndex;
        private bool _done;

        public void Run( )
        {
            while (true)
            {
                var text = EditSubmission();
                if (string.IsNullOrEmpty(text))
                    break;
                _history.Add(text);
                _historyIndex = 0;
                if (!text.Contains(Environment.NewLine) && text.StartsWith('#'))
                    EvaluateMetaCommand(text);
                else
                    EvaluateSubmission(text);
            }
        }

        private sealed class SubmissionView
        {
            private readonly Action<string> _lineRenderer;
            private readonly ObservableCollection<string> _submissionDocument;
            private readonly int _cursorTop;

            private int _renderedLineCount;
            private int _currentLineIndex;
            private int _currentCharacterIndex;

            public SubmissionView(Action<string> lineRenderer, ObservableCollection<string> submissionDocument)
            {
                _lineRenderer = lineRenderer;
                _submissionDocument = submissionDocument;
                _submissionDocument.CollectionChanged += SubmissionDocumentChanged;
                _cursorTop = Console.CursorTop;
                Render();
            }

            private void SubmissionDocumentChanged(object sender, NotifyCollectionChangedEventArgs e) => Render();

            private void Render( )
            {
                Console.SetCursorPosition(0, _cursorTop);
                Console.CursorVisible = false;
                var lineCount = 0;
                foreach (var line in _submissionDocument)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    if (lineCount == 0)
                        Console.Write("» ");
                    else
                        Console.Write("· ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    _lineRenderer(line);
                    Console.WriteLine(new string(' ', Console.WindowWidth - line.Length - 3));
                    lineCount++;
                }
                var numberOfBlankLines = _renderedLineCount - lineCount;
                if (numberOfBlankLines > 0)
                {
                    var blankLine = new string(' ', Console.WindowWidth);
                    for (var i = 0; i < numberOfBlankLines; i++)
                    {
                        Console.SetCursorPosition(0, _cursorTop + lineCount + i);
                        Console.WriteLine(blankLine);
                    }
                }
                _renderedLineCount = lineCount;
                Console.CursorVisible = true;
                UpdateCursorPosition();
            }

            private void UpdateCursorPosition( )
            {
                Console.CursorTop = _cursorTop + _currentLineIndex;
                Console.CursorLeft = 2 + _currentCharacterIndex;
            }

            public int CurrentLineIndex
            {
                get => _currentLineIndex;
                set
                {
                    if (_currentLineIndex == value)
                        return;
                    _currentLineIndex = value;
                    UpdateCursorPosition();
                }
            }

            public int CurrentCharacterIndex
            {
                get => _currentCharacterIndex;
                set
                {
                    if (_currentCharacterIndex == value)
                        return;
                    _currentCharacterIndex = value;
                    UpdateCursorPosition();
                }
            }
        }

        private string EditSubmission( )
        {
            _done = false;
            var document = new ObservableCollection<string>() { "" };
            var view = new SubmissionView(RenderLine, document);
            while (!_done)
            {
                var key = Console.ReadKey(true);
                HandleKey(key, document, view);
            }
            Console.WriteLine();
            return string.Join(Environment.NewLine, document);
        }

        private void HandleKey(ConsoleKeyInfo key, ObservableCollection<string> document, SubmissionView view)
        {
            if (key.Modifiers == default)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        HandleEnter(document, view);
                        break;
                    case ConsoleKey.LeftArrow:
                        HandleLeftArrow(document, view);
                        break;
                    case ConsoleKey.RightArrow:
                        HandleRightArrow(document, view);
                        break;
                    case ConsoleKey.UpArrow:
                        HandleUpArrow(document, view);
                        break;
                    case ConsoleKey.DownArrow:
                        HandleDownArrow(document, view);
                        break;
                    case ConsoleKey.Backspace:
                        HandleBackspace(document, view);
                        break;
                    case ConsoleKey.Delete:
                        HandleDelete(document, view);
                        break;
                    case ConsoleKey.Escape:
                        HandleEscape(document, view);
                        break;
                    case ConsoleKey.Home:
                        HandleHome(document, view);
                        break;
                    case ConsoleKey.End:
                        HandleEnd(document, view);
                        break;
                    case ConsoleKey.Tab:
                        HandleTab(document, view);
                        break;
                    case ConsoleKey.PageUp:
                        HandlePageUp(document, view);
                        break;
                    case ConsoleKey.PageDown:
                        HandlePageDown(document, view);
                        break;
                    default:
                        break;
                }
            }
            else if (key.Modifiers == ConsoleModifiers.Control)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        HandleControlEnter(document, view);
                        break;
                }
            }
            else if (key.Modifiers == ConsoleModifiers.Shift)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Tab:
                        HandleShiftTab(document, view);
                        break;
                }
            }
            if (key.KeyChar >= ' ')
                HandleTyping(document, view, key.KeyChar.ToString());
        }

        private void HandleEnter(ObservableCollection<string> document, SubmissionView view)
        {
            var submissionText = string.Join(Environment.NewLine, document);
            if (submissionText.StartsWith('#') || IsCompleteSubmission(submissionText))
            {
                view.CurrentLineIndex = document.Count - 1;
                view.CurrentCharacterIndex = document[view.CurrentLineIndex].Length;
                _done = true;
            }
            else
            {
                var start = view.CurrentCharacterIndex;
                var line = document[view.CurrentLineIndex];
                var after = line[start..];
                document.Insert(view.CurrentLineIndex + 1, after);
                document[view.CurrentLineIndex] = line[..start];
                view.CurrentCharacterIndex = 0;
                view.CurrentLineIndex++;
            }
        }

        private void HandleControlEnter(ObservableCollection<string> document, SubmissionView view)
        {
            var start = view.CurrentCharacterIndex;
            var line = document[view.CurrentLineIndex];
            var after = line[start..];
            document.Insert(view.CurrentLineIndex + 1, after);
            document[view.CurrentLineIndex] = line[..start];
            view.CurrentCharacterIndex = 0;
            view.CurrentLineIndex++;
        }

        private void HandleLeftArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentCharacterIndex > 0)
                view.CurrentCharacterIndex--;
            else if (view.CurrentLineIndex > 0)
                view.CurrentCharacterIndex = document[--view.CurrentLineIndex].Length;
        }

        private void HandleRightArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentCharacterIndex < document[view.CurrentLineIndex].Length)
                view.CurrentCharacterIndex++;
            else if (view.CurrentLineIndex < document.Count - 1)
            {
                view.CurrentLineIndex++;
                view.CurrentCharacterIndex = 0;
            }
        }

        private void HandleUpArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentLineIndex > 0)
            {
                view.CurrentLineIndex--;
                var lineIndex = view.CurrentLineIndex;
                if (view.CurrentCharacterIndex > document[lineIndex].Length)
                    view.CurrentCharacterIndex = document[lineIndex].Length;
            }
        }

        private void HandleDownArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentLineIndex < document.Count - 1)
            {
                view.CurrentLineIndex++;
                var lineIndex = view.CurrentLineIndex;
                if (view.CurrentCharacterIndex > document[lineIndex].Length)
                    view.CurrentCharacterIndex = document[lineIndex].Length;
            }
        }

        private void HandleBackspace(ObservableCollection<string> document, SubmissionView view)
        {
            var lineIndex = view.CurrentLineIndex;
            var start = view.CurrentCharacterIndex;
            if (start == 0)
            {
                if (lineIndex > 0)
                {
                    var line = document[lineIndex];
                    var lineLength = document[lineIndex - 1].Length;
                    document[lineIndex - 1] = document[lineIndex - 1] + line;
                    document.RemoveAt(lineIndex);
                    view.CurrentLineIndex--;
                    view.CurrentCharacterIndex = lineLength;
                }
                else
                    return;
            }
            else
            {
                document[lineIndex] = document[lineIndex].Remove(start - 1, 1);
                view.CurrentCharacterIndex--;
            }
        }

        private void HandleDelete(ObservableCollection<string> document, SubmissionView view)
        {
            var lineIndex = view.CurrentLineIndex;
            var start = view.CurrentCharacterIndex;
            if (lineIndex < document.Count - 1)
            {
                var nextLine = document[lineIndex + 1];
                document.RemoveAt(lineIndex + 1);
                document[lineIndex] = document[lineIndex] + nextLine;
            }
            else if (start < document[lineIndex].Length)
                document[lineIndex] = document[lineIndex].Remove(start, 1);
        }

        private void HandleEscape(ObservableCollection<string> document, SubmissionView view)
        {
            if (document.Count > 1)
            {
                document.RemoveAt(view.CurrentLineIndex);
                if (view.CurrentCharacterIndex > document[view.CurrentLineIndex].Length)
                    view.CurrentCharacterIndex = document[view.CurrentLineIndex].Length;
            }
            else
            {
                document[view.CurrentLineIndex] = "";
                view.CurrentCharacterIndex = 0;
            }
        }

        private void HandleHome(ObservableCollection<string> document, SubmissionView view)
            => view.CurrentCharacterIndex = 0;

        private void HandleEnd(ObservableCollection<string> document, SubmissionView view)
            => view.CurrentCharacterIndex = document[view.CurrentLineIndex].Length;

        private void HandleTab(ObservableCollection<string> document, SubmissionView view)
        {
            var start = view.CurrentCharacterIndex;
            var remaining = 4 - (start % 4);
            var line = document[view.CurrentLineIndex];
            document[view.CurrentLineIndex] = line.Insert(start, new string(' ', remaining));
            view.CurrentCharacterIndex += remaining;
        }

        private void HandleShiftTab(ObservableCollection<string> document, SubmissionView view)
        {
            int lineIndex = view.CurrentLineIndex;
            var tab = document[lineIndex][0..4];
            if (tab == "    ")
            {
                document[lineIndex] = document[lineIndex][4..];
                if (view.CurrentCharacterIndex > 4)
                    view.CurrentCharacterIndex -= 4;
                else
                    view.CurrentCharacterIndex = 0;
            }
        }

        private void HandlePageUp(ObservableCollection<string> document, SubmissionView view)
        {
            if (--_historyIndex < 0)
                _historyIndex = _history.Count - 1;
            UpdateDocumentFromHistoty(document, view);
        }

        private void HandlePageDown(ObservableCollection<string> document, SubmissionView view)
        {
            if (++_historyIndex >= _history.Count)
                _historyIndex = 0;
            UpdateDocumentFromHistoty(document, view);
        }

        private void UpdateDocumentFromHistoty(ObservableCollection<string> document, SubmissionView view)
        {
            if (_history.Count == 0)
                return;
            document.Clear();
            var text = _history[_historyIndex].Split(Environment.NewLine);
            foreach (var line in text)
                document.Add(line);
            view.CurrentLineIndex = document.Count - 1;
            view.CurrentCharacterIndex = document[view.CurrentLineIndex].Length;
        }

        private void HandleTyping(ObservableCollection<string> document, SubmissionView view, string text)
        {
            var lineIndex = view.CurrentLineIndex;
            var start = view.CurrentCharacterIndex;
            document[lineIndex] = document[lineIndex].Insert(start, text);
            view.CurrentCharacterIndex += text.Length;
        }

        protected void ClearHistory( ) => _history.Clear();

        protected virtual void RenderLine(string line) => Console.Write(line);

        protected virtual void EvaluateMetaCommand(string input)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Invalid command {input}.");
        }

        protected abstract bool IsCompleteSubmission(string text);

        protected abstract void EvaluateSubmission(string text);
    }
}