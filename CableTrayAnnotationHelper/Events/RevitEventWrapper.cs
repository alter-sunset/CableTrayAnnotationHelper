﻿using Autodesk.Revit.UI;

namespace CableTrayAnnotationHelper.Events
{
    public abstract class RevitEventWrapper<TType> : IExternalEventHandler
    {
        private readonly object _lock = new();
        private TType _savedArgs;
        private readonly ExternalEvent _revitEvent;
        protected RevitEventWrapper()
        {
            _revitEvent = ExternalEvent.Create(this);
        }
        public void Execute(UIApplication app)
        {
            TType args;

            lock (_lock)
            {
                args = _savedArgs;
                _savedArgs = default;
            }

            Execute(app, args);
        }
        public string GetName() => GetType().Name;

        public void Raise(TType args)
        {
            lock (_lock)
            {
                _savedArgs = args;
            }

            _revitEvent.Raise();
        }
        public abstract void Execute(UIApplication app, TType args);
    }
}