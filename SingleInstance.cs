using System;
using System.Threading;

namespace MouseGuard
{
    internal sealed class SingleInstance : IDisposable
    {
        private Mutex? _mutex;
        private bool _hasHandle;
        private readonly string _name;

        /// <summary>
        /// True if this process is the first/primary instance.
        /// </summary>
        public bool IsFirstInstance => _hasHandle;

        public SingleInstance(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            try
            {
                // Create a named mutex. The out parameter tells us if we created it (first instance)
                _mutex = new Mutex(initiallyOwned: true, name, out bool createdNew);
                _hasHandle = createdNew;
            }
            catch
            {
                // If we cannot create a mutex for some reason, assume not the first instance
                _hasHandle = false;
            }
        }

        public void Dispose()
        {
            if (_mutex != null)
            {
                if (_hasHandle)
                {
                    try { _mutex.ReleaseMutex(); } catch { }
                    _hasHandle = false;
                }
                _mutex.Dispose();
                _mutex = null;
            }
        }
    }
}
