using System;

namespace fastdee.Stratum
{
    public class NotificationSystem : IDisposable
    {
        public class NewJobReceivedEventArgs : EventArgs
        {
            public readonly Notification.NewJob newJob;
            public NewJobReceivedEventArgs(Notification.NewJob newJob) { this.newJob = newJob; }
        }
        public event EventHandler<NewJobReceivedEventArgs>? NewJobReceived;
        protected virtual void OnNewJobReceived(NewJobReceivedEventArgs args) => NewJobReceived?.Invoke(this, args);
        public class DifficultyReceivedEventArgs : EventArgs
        {
            public readonly double difficulty;
            public DifficultyReceivedEventArgs(double difficulty) { this.difficulty = difficulty; }
        }
        public event EventHandler<DifficultyReceivedEventArgs>? DifficultyReceived;
        protected virtual void OnDifficultyReceived(DifficultyReceivedEventArgs args) => DifficultyReceived?.Invoke(this, args);



        public bool Mangle(string method, object? evargs) => method.Trim() switch
        {
            "mining.notify" => MangleMiningNotify(evargs),
            "mining.set_difficulty" => MangleSetDifficulty(evargs),
            null => throw new MissingRequiredException("notifications must have a method string"),
            "" => throw new MissingRequiredException("notifications must have non-empty method string"),
            _ => false
        };

        bool MangleMiningNotify(object? evargs)
        {
            var res = NotificationParser.MiningNotify(evargs);
            OnNewJobReceived(new NewJobReceivedEventArgs(res));
            return true;
        }

        bool MangleSetDifficulty(object? evargs)
        {
            var res = NotificationParser.SetDifficulty(evargs);
            OnDifficultyReceived(new DifficultyReceivedEventArgs(res));
            return true;
        }

        #region IDisposable support
        bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~NotificationSystem()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
