using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ssapj.YetAnotherCommander
{
    internal class Program
    {
        private static async Task Main()
        {
            var conf = ReadConfig.Instance.Config;

            using (var pool = new BartenderPool(conf.BtwFilePath, conf.ProcessRefreshInterval))
            {
                var datafiles = Directory.GetFiles(conf.ScanFolder, conf.FileMatchPattern, SearchOption.TopDirectoryOnly);

                if (datafiles.Any())
                {
                    foreach (var item in datafiles)
                    {
                        await pool.Print(item);
                        File.Delete(item);
                    }
                }

                var watcher = new FileSystemWatcher()
                {
                    Path = conf.ScanFolder,
                    Filter = conf.FileMatchPattern,
                    NotifyFilter = NotifyFilters.FileName
                };

                watcher.CreatedAsObservable()
                    .Subscribe(async x =>
                    {
                        var textFiles = Directory.GetFiles(conf.ScanFolder, conf.FileMatchPattern, SearchOption.TopDirectoryOnly)
                            .OrderBy(File.GetLastWriteTimeUtc).ToArray();

                        foreach (var item in textFiles)
                        {
                            await pool.Print(item);
                            File.Delete(item);
                        }
                    });

                // 監視開始
                watcher.EnableRaisingEvents = true;

                // 待ち
                Console.ReadLine();
                watcher.Dispose();
            }


        }
    }

    internal static class FileSystemWatcherExtensions
    {
        // ChangedイベントをIObservable<TEventArgs>にするヘルパーメソッド
        public static IObservable<FileSystemEventArgs> ChangedAsObservable(this FileSystemWatcher self)
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (_, e) => h(e),
                h => self.Changed += h,
                h => self.Changed -= h);
        }

        public static IObservable<FileSystemEventArgs> CreatedAsObservable(this FileSystemWatcher self)
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (_, e) => h(e),
                h => self.Created += h,
                h => self.Created -= h);

        }
    }

}
