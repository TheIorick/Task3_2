using System;
using System.Collections.ObjectModel;
using System.Timers;

namespace Task3_2.Models
{
    public class CrossingModel : IDisposable
    {
        // Параметры модели остаются без изменений
        public double CrossingX { get; } = 350;
        public double CrossingWidth { get; } = 80;
        public double RoadY { get; } = 200;
        public double RoadHeight { get; } = 100;
        
        public TrafficLight TrafficLight { get; }
        
        private readonly Timer _updateTimer;
        private readonly Timer _carSpawnTimer;
        private readonly Timer _pedestrianSpawnTimer;
        
        public ObservableCollection<Car> Cars { get; }
        public ObservableCollection<Pedestrian> Pedestrians { get; }
        
        private readonly Random _random = new Random();
        
        public event EventHandler<Car> AccidentOccurred;

        public CrossingModel()
        {
            TrafficLight = new TrafficLight();
            
            Cars = new ObservableCollection<Car>();
            Pedestrians = new ObservableCollection<Pedestrian>();
            
            _updateTimer = new Timer(50);
            _updateTimer.Elapsed += UpdateObjects;
            _updateTimer.AutoReset = true;
            _updateTimer.Start();
            
            _carSpawnTimer = new Timer(2000);
            _carSpawnTimer.Elapsed += SpawnCar;
            _carSpawnTimer.AutoReset = true;
            _carSpawnTimer.Start();
            
            _pedestrianSpawnTimer = new Timer(3000);
            _pedestrianSpawnTimer.Elapsed += SpawnPedestrian;
            _pedestrianSpawnTimer.AutoReset = true;
            _pedestrianSpawnTimer.Start();
        }

        private void UpdateObjects(object sender, ElapsedEventArgs e)
        {
            // Обновление светофора
            TrafficLight.Update();
            
            // Безопасное обновление машин
            for (int i = Cars.Count - 1; i >= 0; i--)
            {
                if (i < Cars.Count)
                {
                    Cars[i].Update();
                }
            }
            
            // Безопасное обновление пешеходов
            for (int i = Pedestrians.Count - 1; i >= 0; i--)
            {
                if (i < Pedestrians.Count)
                {
                    Pedestrians[i].Update();
                }
            }
        }

        private void SpawnCar(object sender, ElapsedEventArgs e)
        {
            // Создаем обычную машину
            bool isEmergency = _random.Next(0, 100) < 5; // Редкое появление аварийной службы в обычном режиме
            var car = new Car(this, isEmergency);
            Cars.Add(car);
        }

        private void SpawnPedestrian(object sender, ElapsedEventArgs e)
        {
            var pedestrian = new Pedestrian(this);
            Pedestrians.Add(pedestrian);
        }

        public void RemoveCar(Car car)
        {
            if (Cars.Contains(car))
            {
                Cars.Remove(car);
            }
        }

        public void RemovePedestrian(Pedestrian pedestrian)
        {
            if (Pedestrians.Contains(pedestrian))
            {
                Pedestrians.Remove(pedestrian);
            }
        }

        public void TriggerAccident(Car car)
        {
            // Генерируем событие аварии
            AccidentOccurred?.Invoke(this, car);
            
            // Создаем аварийную машину и добавляем ее на сцену
            // Она появится с правой или левой стороны с более высокой скоростью
            var emergencyCar = new Car(this, true);
            Cars.Add(emergencyCar);
        }

        public void Dispose()
        {
            _updateTimer.Stop();
            _updateTimer.Dispose();
            _carSpawnTimer.Stop();
            _carSpawnTimer.Dispose();
            _pedestrianSpawnTimer.Stop();
            _pedestrianSpawnTimer.Dispose();
        }
    }
}