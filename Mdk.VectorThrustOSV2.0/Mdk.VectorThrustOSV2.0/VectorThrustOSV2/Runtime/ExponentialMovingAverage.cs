namespace IngameScript.VectorThrustOSV2.Runtime
{
    // Performance monitoring class that tracks the average and maximum runtime of the script.
    internal class ExponentialMovingAverage
    {
        private bool _isInitialized;
        private readonly double _weightingMultiplier;
        private double _previousAverage;

        public double Average { get; private set; }
        public double Slope { get; private set; }

        public ExponentialMovingAverage(int lookback) => _weightingMultiplier = 2.0 / (lookback + 1);

        public void AddDataPoint(double dataPoint)
        {
            if (!_isInitialized)
            {
                Average             = dataPoint;
                Slope               = 0;
                _previousAverage    = Average;
                _isInitialized      = true;
                return;
            }

            // Calculate the new EMA by applying the weighting multiplier to the difference between the new data point and the previous average.
            // This gives more weight to recent data, smoothing out fluctuations while still reacting to changes.
            Average = ((dataPoint - _previousAverage) * _weightingMultiplier) + _previousAverage;

            // Determine the slope (rate of change) by comparing the new average to the previous one.
            // A positive slope indicates an upward trend; negative means a downward trend.
            Slope = Average - _previousAverage;

            // Store the new average as the previous value for the next iteration.
            _previousAverage = Average;
        }
    }
}
