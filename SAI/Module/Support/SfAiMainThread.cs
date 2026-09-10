using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFramework.SAI.Module.Support
{
    /// <summary>把回调派发到 Unity 主线程（Agent 工具需要）</summary>
    public static class SfAiMainThread
    {
        static SynchronizationContext _context;
        static Func<Action, bool> _editorPoster;

        public static void Capture() => _context = SynchronizationContext.Current;

        public static void SetContext(SynchronizationContext context) => _context = context;

        /// <summary>Editor 可注入 delayCall 派发器</summary>
        public static void SetEditorPoster(Func<Action, bool> poster) => _editorPoster = poster;

        public static Task<T> RunAsync<T>(Func<T> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            var ctx = _context ?? SynchronizationContext.Current;
            if (ctx != null)
            {
                if (ReferenceEquals(SynchronizationContext.Current, ctx))
                    return Task.FromResult(func());

                var tcs = new TaskCompletionSource<T>();
                ctx.Post(_ =>
                {
                    try { tcs.TrySetResult(func()); }
                    catch (Exception e) { tcs.TrySetException(e); }
                }, null);
                return tcs.Task;
            }

            if (_editorPoster != null)
            {
                var tcs = new TaskCompletionSource<T>();
                _editorPoster(() =>
                {
                    try { tcs.TrySetResult(func()); }
                    catch (Exception e) { tcs.TrySetException(e); }
                });
                return tcs.Task;
            }

            return Task.FromResult(func());
        }

        public static Task RunAsync(Action action) =>
            RunAsync(() =>
            {
                action();
                return true;
            });
    }
}
