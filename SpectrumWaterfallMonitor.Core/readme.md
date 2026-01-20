# SpectrumWaterfallMonitor.Core

Core-модуль містить всю бізнес-логіку

## Структура
```
Core
├── Enums
├── Models
├── Records
├── Rendering
└── Mvvm
```

## Enums
- LiveSpectrumGeneratorFrameType - тип кадру генератора
- SpectrumPeakDriftMode - режим дрейфу піка на графіку

## Models
- SpectrumFrame - DTO одного спектрального фрейму
- ColorGradient - модель градієнта

## Records
- RgbColor - RGB-колір
- GradientStopModel - точка градієнта
- SpectrumGeneratorOptions - конфіг генератора
- SpectrumPeakOptions - параметри піка графіку

## Rendering
- SpectrumGenerator - генератор спектральних фреймів
- PeakState - стан піка
- GradientLookup - мапінг градієнта

## MVVM
- ViewModelBase - реалізація INotifyPropertyChanged
- RelayCommand - реалізація ICommand
