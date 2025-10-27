using System;
using H5.Core;
using static H5.Core.dom;


namespace Tesserae
{
    public class DebouncerWithMaxDelay
    {
        private double _refreshTimeout = 0;
        private int    _delayInMs;
        private int    _maxDelayInMs;
        private double _lastInvoked = 0;
        private Action _onTrigger;

        public DebouncerWithMaxDelay(Action onTrigger, int delayInMs = 16, int maxDelayInMs = -1)
        {
            if (maxDelayInMs < 0) maxDelayInMs = delayInMs;

            if (delayInMs >= maxDelayInMs) maxDelayInMs = delayInMs * 10;

            _delayInMs    = delayInMs;
            _maxDelayInMs = maxDelayInMs;
            _onTrigger    = onTrigger;
        }
        public int DelayInMs => _delayInMs;

        public void RaiseOnValueChanged()
        {
            window.clearTimeout(_refreshTimeout);

            if (_refreshTimeout > 0 && es5.Date.now() > (_lastInvoked + _maxDelayInMs))
            {
                _onTrigger();
                _lastInvoked    = es5.Date.now();
                _refreshTimeout = 0;
            }
            else
            {
                if(_lastInvoked == 0) _lastInvoked = es5.Date.now(); //Need to set it here too to avoid immediatelly triggering it above on two subsequent calls
                _refreshTimeout = window.setTimeout(
                    _ =>
                    {
                        _onTrigger();
                        _lastInvoked    = es5.Date.now();
                        _refreshTimeout = 0;
                    },
                    _delayInMs
                );
            }
        }
    }
}