using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Task3_2.Models
{
    public class TrafficLight
    {
        public enum LightState
        {
            RedForPedestrian,
            GreenForPedestrian
        }

        private Timer _timer;
        private LightState _currentState;

        public LightState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    StateChanged?.Invoke(this, _currentState);
                }
            }
        }

        public event EventHandler<LightState> StateChanged;

        public TrafficLight()
        {
            _currentState = LightState.RedForPedestrian;
            _timer = new Timer(5000); // 5 секунд для каждого состояния
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Переключаем состояние светофора
            CurrentState = CurrentState == LightState.RedForPedestrian 
                ? LightState.GreenForPedestrian 
                : LightState.RedForPedestrian;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}