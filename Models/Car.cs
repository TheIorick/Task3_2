using System;

namespace Task3_2.Models
{
    public class Car
    {
        private readonly Random _random = new Random();
        private readonly CrossingModel _crossing;
        
        // Координаты автомобиля
        public double X { get; private set; }
        public double Y { get; private set; }
        
        // Скорость движения
        private double _speed;
        private readonly double _maxSpeed;
        
        // Направление движения (вправо = true, влево = false)
        private bool _movingRight;
        
        // Полоса движения (верхняя = 0, нижняя = 1)
        private int _lane;
        
        // Счетчик времени для попытки смены полосы
        private int _laneChangeCounter;
        
        // Является ли машина аварийной службой
        public bool IsEmergency { get; }

        // Стандартный конструктор для создания машины через модель перекрёстка
        public Car(CrossingModel crossing, bool isEmergency = false)
        {
            _crossing = crossing;
            IsEmergency = isEmergency;
            
            _maxSpeed = isEmergency ? 8 : _random.Next(3, 6);
            _speed = _maxSpeed;
            _movingRight = _random.Next(0, 2) == 0;
            
            // Выбираем случайную полосу движения
            _lane = _random.Next(0, 2);
            
            // Машины начинают движение с краев экрана
            X = _movingRight ? -50 : 850;
            
            // Размещение на дороге в зависимости от полосы
            double laneOffset = _lane == 0 ? 20 : 60;
            Y = _crossing.RoadY + laneOffset;
            
            _laneChangeCounter = 0;
        }
        
        // Перегруженный конструктор для создания машины с конкретными координатами
        // (используется аварийной службой)
        public Car(double x, double y, bool isEmergency)
        {
            X = x;
            Y = y;
            IsEmergency = isEmergency;
            _maxSpeed = isEmergency ? 8 : _random.Next(3, 6);
            _speed = _maxSpeed;
            _movingRight = x < 400; // Если машина слева, движется вправо
            _crossing = null; // В этом случае модель перекрёстка не используется
            
            _lane = y < 230 ? 0 : 1; // Определяем полосу по Y-координате
            _laneChangeCounter = 0;
        }

        public void Update()
        {
            // Для машин без перекрёстка просто двигаемся
            if (_crossing == null)
            {
                X += _movingRight ? _speed : -_speed;
                return;
            }
            
            // Восстанавливаем скорость постепенно, если нет препятствий
            if (_speed < _maxSpeed)
            {
                _speed += 0.1;
                if (_speed > _maxSpeed)
                    _speed = _maxSpeed;
            }
            
            bool needToSlow = false;
            
            // Проверка светофора - только если приближаемся к переходу
            if (!IsEmergency && 
                _crossing.TrafficLight.CurrentState == TrafficLight.LightState.GreenForPedestrian &&
                IsApproachingCrossing())
            {
                needToSlow = true;
            }
            
            // Проверяем наличие машин впереди
            Car carAhead = FindCarAhead();
            if (carAhead != null)
            {
                // Вычисляем дистанцию до машины впереди
                double distance = _movingRight 
                    ? carAhead.X - X - 40 // 40 - длина машины
                    : X - carAhead.X - 40;
                    
                // Если дистанция меньше безопасной, замедляемся
                if (distance < 50)
                {
                    needToSlow = true;
                    
                    // Пробуем сменить полосу каждые 100 тиков
                    _laneChangeCounter++;
                    if (_laneChangeCounter > 100 && _random.Next(0, 10) < 3) // 30% шанс
                    {
                        TryChangeLane();
                        _laneChangeCounter = 0;
                    }
                }
            }
            
            // Если нужно снизить скорость
            if (needToSlow)
            {
                _speed *= 0.9; // Постепенное снижение скорости
                if (_speed < 0.5) _speed = 0.5; // Минимальная скорость
            }
            
            // Движение машины
            X += _movingRight ? _speed : -_speed;
            
            // Удаляем машину, если она ушла за пределы экрана
            if (X < -60 || X > 860)
            {
                _crossing.RemoveCar(this);
                return;
            }
            
            // Проверка столкновения с пешеходами
            CheckPedestrianCollision();
        }

        // Попытка сменить полосу движения
        private void TryChangeLane()
        {
            // Меняем полосу на противоположную
            int newLane = _lane == 0 ? 1 : 0;
            
            // Проверяем, свободна ли новая полоса
            bool laneIsFree = true;
            foreach (var otherCar in _crossing.Cars)
            {
                if (otherCar != this)
                {
                    // Проверяем машины на той же стороне и близко по горизонтали
                    double horizontalDistance = Math.Abs(X - otherCar.X);
                    if (horizontalDistance < 60 && otherCar._lane == newLane)
                    {
                        laneIsFree = false;
                        break;
                    }
                }
            }
            
            // Если полоса свободна, меняем её
            if (laneIsFree)
            {
                _lane = newLane;
                double laneOffset = _lane == 0 ? 20 : 60;
                Y = _crossing.RoadY + laneOffset;
            }
        }
        
        // Поиск ближайшей машины впереди
        private Car FindCarAhead()
        {
            Car closest = null;
            double minDistance = double.MaxValue;
            
            foreach (var otherCar in _crossing.Cars)
            {
                if (otherCar != this && otherCar._lane == _lane)
                {
                    // Если другая машина находится впереди в том же направлении
                    if (_movingRight && otherCar.X > X || !_movingRight && otherCar.X < X)
                    {
                        double distance = Math.Abs(X - otherCar.X);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closest = otherCar;
                        }
                    }
                }
            }
            
            return closest;
        }

        private bool IsApproachingCrossing()
        {
            if (_crossing == null) return false;
            
            // Расстояние до перехода, при котором машина начинает реагировать
            double reactionDistance = _speed * 15; // Зависит от скорости
            
            if (_movingRight)
            {
                return X + reactionDistance > _crossing.CrossingX && X < _crossing.CrossingX;
            }
            else
            {
                return X - reactionDistance < _crossing.CrossingX + _crossing.CrossingWidth && 
                       X > _crossing.CrossingX + _crossing.CrossingWidth;
            }
        }

        private void CheckPedestrianCollision()
        {
            if (_crossing == null) return;
            
            foreach (var pedestrian in _crossing.Pedestrians)
            {
                // Более точная проверка столкновения
                double dx = Math.Abs(X + 20 - pedestrian.X);
                double dy = Math.Abs(Y + 10 - pedestrian.Y);
                
                // Если пешеход и машина достаточно близко друг к другу
                if (dx < 18 && dy < 13)
                {
                    // На переходе и при красном для пешеходов - риск аварии
                    bool onCrossing = pedestrian.X >= _crossing.CrossingX && 
                                    pedestrian.X <= _crossing.CrossingX + _crossing.CrossingWidth;
                                    
                    if (onCrossing && _crossing.TrafficLight.CurrentState == TrafficLight.LightState.RedForPedestrian)
                    {
                        // Шанс аварии 60%
                        if (_random.Next(0, 100) < 60) 
                        {
                            _crossing.TriggerAccident(this);
                            break;
                        }
                    }
                }
            }
        }
    }
}