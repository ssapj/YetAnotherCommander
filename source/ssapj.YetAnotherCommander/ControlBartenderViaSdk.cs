using Seagull.BarTender.Print;
using Seagull.BarTender.Print.Database;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ssapj.YetAnotherCommander
{
    internal class ControlBartenderViaSdk : IDisposable
    {
        private Engine _bartenderEngine;
        private LabelFormatDocument _labelFormatDocument;
        private (bool doesUseTextDatabase, string databaseConnectionName) _databaseInformation;
        private DatabaseConnection _databaseConnection;

        public bool IsReady { get; private set; }

        public ControlBartenderViaSdk()
        {
            this._bartenderEngine = new Engine(true);
            this._bartenderEngine.Window.VisibleWindows = VisibleWindows.InteractiveDialogs;
        }

        public ControlBartenderViaSdk(string btwFilePath)
        {
            if (File.Exists(btwFilePath))
            {
                this._bartenderEngine = new Engine(true);
                this._labelFormatDocument = this._bartenderEngine.Documents.Open(btwFilePath);

                if (this._labelFormatDocument.DatabaseConnections.Any())
                {
                    if (this._labelFormatDocument.DatabaseConnections.Any(x => x.Type == InputDataFile.TextFile))
                    {
                        this._databaseInformation.doesUseTextDatabase = true;
                        this._databaseConnection = this._labelFormatDocument.DatabaseConnections.First(x => x.Type == InputDataFile.TextFile);
                        this._databaseInformation.databaseConnectionName = this._databaseConnection.Name;
                    }
                    else
                    {
                        this._databaseInformation.doesUseTextDatabase = false;
                    }
                }
                else
                {
                    this._databaseInformation.doesUseTextDatabase = false;
                }

                this._bartenderEngine.Window.VisibleWindows = VisibleWindows.InteractiveDialogs;
            }
            else
            {
                throw new FileNotFoundException(btwFilePath);
            }
        }

        public ControlBartenderViaSdk(bool isAsnyc, string btwFilePath)
        {
            if (File.Exists(btwFilePath))
            {
                if (isAsnyc)
                {
                    this.StartEngineAsync(btwFilePath);
                }
                else
                {
                    this.StartEngine(btwFilePath);
                }
            }
            else
            {
                throw new FileNotFoundException(btwFilePath);
            }
        }

        private void StartEngine(string btwFilePath)
        {
            this._bartenderEngine = new Engine(true);
            this._labelFormatDocument = this._bartenderEngine.Documents.Open(btwFilePath);

            if (this._labelFormatDocument.DatabaseConnections.Any())
            {
                if (this._labelFormatDocument.DatabaseConnections.Any(x => x.Type == InputDataFile.TextFile))
                {
                    this._databaseInformation.doesUseTextDatabase = true;
                    this._databaseConnection = this._labelFormatDocument.DatabaseConnections.First(x => x.Type == InputDataFile.TextFile);
                    this._databaseInformation.databaseConnectionName = this._databaseConnection.Name;
                }
                else
                {
                    this._databaseInformation.doesUseTextDatabase = false;
                }
            }
            else
            {
                this._databaseInformation.doesUseTextDatabase = false;
            }

            this._bartenderEngine.Window.VisibleWindows = VisibleWindows.InteractiveDialogs;
            this.IsReady = true;
        }

        private async void StartEngineAsync(string btwFilePath)
        {
            await Task.Run(() => this.StartEngine(btwFilePath)).ConfigureAwait(false);
        }

        public bool? Print()
        {
            var result = this._labelFormatDocument.Print("", -1);

            switch (result)
            {
                case Result.Success:
                    return true;
                case Result.Timeout:
                    return null;
                case Result.Failure:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool? Print(string textFilePath)
        {
            if (File.Exists(textFilePath))
            {
                if (this._databaseInformation.doesUseTextDatabase)
                {
                    this._databaseConnection.FileName = textFilePath;
                    return this.Print();
                }

                throw new ArgumentException("This document cannot use a text database.");
            }

            throw new FileNotFoundException(textFilePath);
        }

        #region IDisposable Support
        private bool _disposedValue = false; 

        protected virtual async Task Dispose(bool disposing)
        {
            if (this._disposedValue)
            {
                return;
            }

            if (disposing)
            {
                //起動途中のプロセスは起動しきるまで待ってやらないとNullReferenceExceptionを吐くので待ってやる
                while (!this.IsReady)
                {
                    await Task.Delay(1000);
                }

                this._bartenderEngine.Dispose();
            }

            this._disposedValue = true;
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            this.Dispose(true).Wait();
        }
        #endregion
    }
}
