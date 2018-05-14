using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ssapj.YetAnotherCommander
{
    internal class BartenderEngine : IDisposable
    {
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly IDisposable _timer;
        private readonly CompositeDisposable _compositeDisposable;
        private readonly ControlBartenderViaSdk _engine;

        public uint LifeTime { get; private set; }

        public bool IsReady => this._engine != null && this._engine.IsReady;

        public bool IsWorking { get; private set; }

        public BartenderEngine(string btwFilePath)
        {
            this.IsWorking = false;

            this._compositeDisposable = new CompositeDisposable();

            if (File.Exists(btwFilePath))
            {
                this._engine = new ControlBartenderViaSdk(true, btwFilePath);
                this._compositeDisposable.Add(this._engine);
            }
            else
            {
                throw new FileNotFoundException(btwFilePath);
            }

            this._timer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                .Subscribe(_ => this.LifeTime += this._engine.IsReady ? (uint)1 : (uint)0);

            this._compositeDisposable.Add(this._timer);
        }

        public bool? Print(string textFilePath)
        {
            if (File.Exists(textFilePath))
            {
                this.IsWorking = true;
                var result =  this._engine.Print(textFilePath);
                this.IsWorking = false;
                return result;
            }

            throw new FileNotFoundException(textFilePath);
        }

        #region IDisposable Support
        private bool _disposedValue = false; 

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposedValue)
            {
                return;
            }

            if (disposing)
            {
                this._compositeDisposable.Dispose();
            }

            this._disposedValue = true;
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            this.Dispose(true);
        }
        #endregion

    }
}
