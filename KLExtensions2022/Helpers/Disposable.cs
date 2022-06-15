using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLExtensions2022
{
    public sealed class Disposable : IDisposable
    {
        Action onDispose;

        public Disposable(Action disposer) : this(disposer, false) { }
        public Disposable(Action disposer, bool repeatable)
        {
            if (disposer == null) throw new ArgumentNullException("disposer");

            onDispose = disposer;
            Repeatable = repeatable;
        }

        public bool Repeatable { get; private set; }
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            if (Disposed && !Repeatable) 
                return;
            Disposed = true;
            onDispose();
            if (!Repeatable) 
                onDispose = null;              
        }
    }
}
