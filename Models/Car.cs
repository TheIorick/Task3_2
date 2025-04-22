using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Task3_2.Models
{
    public class Car
    {
        private static readonly Random _random = new Random();
        
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; }
        public bool IsEmergency { get; set; }
        public bool IsStopped { get; set; }

        public Car(double x, double y, bool isEmergency = false)
        {
            X = x;
            Y = y;
            Speed = isEmergency ? 3.0 : _random.NextDouble() * 1.5 + 0.5; // Скорость от 0.5 до 2.0
            IsEmergency = isEmergency;
            IsStopped = false;
        }

        public void Move(TrafficLight trafficLight, double crossingStart, double crossingEnd)
        {
            if (IsStopped) return;

            X += Speed;

            // Проверка, должна ли машина остановиться на пешеходном переходе
            if (!IsEmergency && 
                trafficLight.CurrentState == TrafficLight.LightState.GreenForPedestrian &&
                X > crossingStart - 30 && X < crossingStart)
            {
                IsStopped = true;
            }

            // Если машина проехала переход, она может продолжать движение
            if (IsStopped && X > crossingEnd)
            {
                IsStopped = false;
            }
        }

        // Метод для проверки, вызвала ли машина аварийную ситуацию
        public bool CheckAccident(double crossingStart, double crossingEnd)
        {
            // Если машина проезжает на красный для пешеходов свет, есть шанс аварии
            if (!IsEmergency && X > crossingStart && X < crossingEnd && _random.NextDouble() < 0.01)
                return true;
            return false;
        }
    }
}
