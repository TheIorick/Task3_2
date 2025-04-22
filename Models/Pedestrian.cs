using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Task3_2.Models
{
    public class Pedestrian
    {
        private static readonly Random _random = new Random();
        
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; }
        public bool IsWaiting { get; set; }
        public bool IsCrossing { get; set; }

        public Pedestrian(double x, double y)
        {
            X = x;
            Y = y;
            Speed = _random.NextDouble() * 0.7 + 0.3; // Скорость от 0.3 до 1.0
            IsWaiting = true;
            IsCrossing = false;
        }

        public void Update(TrafficLight trafficLight, double roadY, double roadHeight)
        {
            // Если свет зеленый для пешеходов, начинаем пересекать дорогу
            if (trafficLight.CurrentState == TrafficLight.LightState.GreenForPedestrian && IsWaiting)
            {
                IsWaiting = false;
                IsCrossing = true;
            }

            // Если пешеход переходит дорогу
            if (IsCrossing)
            {
                Y += Speed;
                
                // Если пешеход перешел дорогу
                if (Y > roadY + roadHeight)
                {
                    IsCrossing = false;
                }
            }
        }
    }
}