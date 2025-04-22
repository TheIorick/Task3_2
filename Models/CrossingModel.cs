using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Threading;

namespace Task3_2.Models
{
    public class CrossingModel
    {
        private readonly Random _random = new Random();
        private readonly object _lockObject = new object();
        private CancellationTokenSource _cancellationTokenSource;
        
        public TrafficLight TrafficLight { get; }
        public ObservableCollection<Car> Cars { get; }
        public ObservableCollection<Pedestrian> Pedestrians { get; }
        public IEmergencyService EmergencyService { get; }
        
        public double CrossingX { get; } = 300;
        public double CrossingWidth { get; } = 50;
        public double RoadY { get; } = 150;
        public double RoadHeight { get; } = 100;
        
        public event EventHandler<Car> AccidentOccurred;

        public CrossingModel()
        {
            TrafficLight = new TrafficLight();
            Cars = new ObservableCollection<Car>();
            Pedestrians = new ObservableCollection<Pedestrian>();
            EmergencyService = new EmergencyService();
            
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Подписываемся на событие изменения состояния светофора
            TrafficLight.StateChanged += OnTrafficLightStateChanged;
            
            // Запускаем фоновый поток для генерации машин
            Task.Run(() => GenerateCarsAsync(_cancellationTokenSource.Token));
            
            // Запускаем фоновый поток для генерации пешеходов
            Task.Run(() => GeneratePedestriansAsync(_cancellationTokenSource.Token));
            
            // Запускаем фоновый поток для обновления моделей
            Task.Run(() => UpdateModelsAsync(_cancellationTokenSource.Token));
            
            // Запускаем светофор
            TrafficLight.Start();
        }

        private async Task GenerateCarsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Создаем новую машину
                var newCar = new Car(-50, RoadY + RoadHeight / 2);
                
                await Task.Delay(_random.Next(2000, 5000), cancellationToken);
                
                lock (_lockObject)
                {
                    Cars.Add(newCar);
                }
                
                // Удаляем машины, которые уехали за пределы экрана
                RemoveOffscreenCars();
            }
        }

        private async Task GeneratePedestriansAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Создаем нового пешехода
                var newPedestrian = new Pedestrian(CrossingX + CrossingWidth / 2, RoadY - 10);
                
                await Task.Delay(_random.Next(3000, 8000), cancellationToken);
                
                lock (_lockObject)
                {
                    Pedestrians.Add(newPedestrian);
                }
                
                // Удаляем пешеходов, которые ушли за пределы экрана
                RemoveOffscreenPedestrians();
            }
        }

        private async Task UpdateModelsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(50, cancellationToken); // Обновление каждые 50 мс
                
                lock (_lockObject)
                {
                    // Обновление машин
                    foreach (var car in Cars.ToList())
                    {
                        car.Move(TrafficLight, CrossingX, CrossingX + CrossingWidth);
                        
                        // Проверка на аварийную ситуацию
                        if (car.CheckAccident(CrossingX, CrossingX + CrossingWidth))
                        {
                            AccidentOccurred?.Invoke(this, car);
                            
                            // Вызов аварийной службы
                            Task.Run(() => EmergencyService.RespondToAccident(car.X, car.Y));
                        }
                    }
                    
                    // Обновление пешеходов
                    foreach (var pedestrian in Pedestrians.ToList())
                    {
                        pedestrian.Update(TrafficLight, RoadY, RoadHeight);
                    }
                }
            }
        }

        private void OnTrafficLightStateChanged(object sender, TrafficLight.LightState state)
        {
            // Дополнительная логика при изменении состояния светофора
        }

        private void RemoveOffscreenCars()
        {
            lock (_lockObject)
            {
                for (int i = Cars.Count - 1; i >= 0; i--)
                {
                    if (Cars[i].X > 800) // Ширина окна
                    {
                        Cars.RemoveAt(i);
                    }
                }
            }
        }

        private void RemoveOffscreenPedestrians()
        {
            lock (_lockObject)
            {
                for (int i = Pedestrians.Count - 1; i >= 0; i--)
                {
                    if (Pedestrians[i].Y > RoadY + RoadHeight + 50)
                    {
                        Pedestrians.RemoveAt(i);
                    }
                }
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            TrafficLight.Stop();
        }
    }
}