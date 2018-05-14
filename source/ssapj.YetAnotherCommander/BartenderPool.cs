using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ssapj.YetAnotherCommander
{
    internal class BartenderPool : IDisposable
    {
        private readonly List<(uint index, BartenderEngine engine)> _pool;
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		// ReSharper disable once FieldCanBeMadeReadOnly.Local
		private string _btwFilePath;
        private readonly IDisposable _timer;

        public BartenderPool(string btwFilePath, uint cycleSec = 3600)
        {
            var indexer = SingletonIndexer.Instance;
            this._pool = new List<(uint index, BartenderEngine engine)>();
            var prepareSec = cycleSec - 90;

            if (File.Exists(btwFilePath))
            {
                this._btwFilePath = btwFilePath;
            }
            else
            {
                throw new FileNotFoundException(btwFilePath);
            }


            this._timer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    if (this._pool.Count == 0)
                    {
                        this._pool.Add((indexer.GetValue(), new BartenderEngine(this._btwFilePath)));
                    }
                    else
                    {
                        if (this._pool.All(x => prepareSec < x.engine.LifeTime))
                        {
                            this._pool.Add((indexer.GetValue(), new BartenderEngine(this._btwFilePath)));
                        }

                        var p = this._pool.Where(x => cycleSec < x.engine.LifeTime && !x.engine.IsWorking)
                            .Select(x =>
                            {
                                x.engine.Dispose();
                                return x.index;
                            });

                        if (p.Any())
                        {
                            this._pool.RemoveAll(x => p.Contains(x.index));
                        }

                    }

                });
        }

        public async Task<bool?> Print(string textFilePath)
        {
            //準備できているものがなければ待つ
            while (this._pool.Count == 0 || this._pool.Select(x => x.engine).All(x => !x.IsReady || x.IsWorking))
            {
                await Task.Delay(1500);
            }

            return this._pool.Select(x => x.engine).Where(x => x.IsReady && !x.IsWorking).OrderBy(x => x.LifeTime).First().Print(textFilePath);
        }


        #region IDisposable Support
        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposedValue)
            {
                return;
            }

            if (disposing)
            {
                this._pool.ForEach(x => x.engine.Dispose());
                this._timer.Dispose();
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
