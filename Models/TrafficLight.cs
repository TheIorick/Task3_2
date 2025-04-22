using System;

namespace Task3_2.Models
{
    public class TrafficLight
    {
        public enum LightState
        {
            RedForPedestrian,      // Красный для пешеходов (зеленый для машин)
            GreenForPedestrian     // Зеленый для пешеходов (красный для машин)
        }

        private LightState _currentState;
        private readonly Random _random = new Random();
        private DateTime _lastStateChange;
        
        private readonly int _minStateTime = 8000;  // 8 секунд
        private readonly int _maxStateTime = 12000; // 12 секунд
        private int _currentStateDuration;

        public LightState CurrentState => _currentState;
        
        public event EventHandler<LightState> StateChanged;

        public TrafficLight()
        {
            _currentState = LightState.GreenForPedestrian; // Начинаем с зеленого для пешеходов
            _lastStateChange = DateTime.Now;
            _currentStateDuration = _random.Next(_minStateTime, _maxStateTime);
        }

        public void Update()
        {
            if ((DateTime.Now - _lastStateChange).TotalMilliseconds >= _currentStateDuration)
            {
                // Меняем сигнал светофора
                _currentState = _currentState == LightState.GreenForPedestrian 
                    ? LightState.RedForPedestrian 
                    : LightState.GreenForPedestrian;
                    
                _lastStateChange = DateTime.Now;
                _currentStateDuration = _random.Next(_minStateTime, _maxStateTime);
                
                // Уведомляем подписчиков о смене сигнала
                StateChanged?.Invoke(this, _currentState);
            }
        }
    }
}