using System;

namespace Task3_2.Models
{
    public class Pedestrian
    {
        private readonly Random _random = new Random();
        private readonly CrossingModel _crossing;
        
        public double X { get; private set; }
        public double Y { get; private set; }
        
        private bool _movingDown;
        private readonly double _speed;
        private bool _isWaiting;

        public Pedestrian(CrossingModel crossing)
        {
            _crossing = crossing;
            _speed = _random.Next(1, 3);
            _movingDown = _random.Next(0, 2) == 0;
            
            Y = _movingDown ? _crossing.RoadY - 20 : _crossing.RoadY + _crossing.RoadHeight + 5;
            X = _crossing.CrossingX + _random.Next(0, (int)_crossing.CrossingWidth);
            
            // Пешеход ждет, если для пешеходов горит красный
            _isWaiting = _crossing.TrafficLight.CurrentState == TrafficLight.LightState.RedForPedestrian;
        }

        public void Update()
        {
            // Пешеходы идут на зеленый для пешеходов, стоят на красный
            if (_crossing.TrafficLight.CurrentState == TrafficLight.LightState.GreenForPedestrian)
            {
                _isWaiting = false;
            }
            else if (IsOnCrossing())
            {
                // Если пешеход уже на переходе и загорелся красный для пешеходов - он должен ждать
                _isWaiting = true;
            }

            if (!_isWaiting)
            {
                Y += _movingDown ? _speed : -_speed;
                
                if (Y < 0 || Y > 450)
                {
                    _crossing.RemovePedestrian(this);
                }
            }
        }

        private bool IsOnCrossing()
        {
            return X >= _crossing.CrossingX && 
                   X <= _crossing.CrossingX + _crossing.CrossingWidth &&
                   Y >= _crossing.RoadY && 
                   Y <= _crossing.RoadY + _crossing.RoadHeight;
        }
    }
}