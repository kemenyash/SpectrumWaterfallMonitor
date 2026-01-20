using SpectrumWaterfallMonitor.Core.Models;
using SpectrumWaterfallMonitor.Core.Mvvm;
using SpectrumWaterfallMonitor.Core.Records;
using SpectrumWaterfallMonitor.Core.Rendering;
using SpectrumWaterfallMonitor.Wpf.Mocks;
using SpectrumWaterfallMonitor.Wpf.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace SpectrumWaterfallMonitor.Wpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private SpectrumGeneratorOptions generationOptions;
        private SpectrumGenerator generator;
        private DispatcherTimer timer;
        private TimeSpan timerInterval;

        private bool isRunning;
        public bool IsRunning
        {
            get => isRunning;
            private set
            {
                if (SetProperty(ref isRunning, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private SpectrumFrame currentFrame;
        public SpectrumFrame CurrentFrame
        {
            get => currentFrame;
            private set => SetProperty(ref currentFrame, value);
        }

        private ObservableCollection<PresetItem> presets;
        public ObservableCollection<PresetItem> Presets
        {
            get => presets;
            private set => SetProperty(ref presets, value);
        }

        private PresetItem selectedPreset;
        public PresetItem SelectedPreset
        {
            get => selectedPreset;
            set
            {
                if (SetProperty(ref selectedPreset, value))
                {
                    ApplyPreset(selectedPreset);
                }
            }
        }

        private int historyLineCount;
        public int HistoryLineCount
        {
            get => historyLineCount;
            set => SetProperty(ref historyLineCount, value);
        }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        public MainViewModel()
        {
            StartCommand = new RelayCommand(Start, CanStart);
            StopCommand = new RelayCommand(Stop, CanStop);

            Initialization();
        }

        private void Initialization()
        {
            IsRunning = false;
            HistoryLineCount = 200;

            Presets = new ObservableCollection<PresetItem>
            {
                new PresetItem("Радіоефір (90–110 МГц, 20 Гц)", SpectrumGeneratorPresets.RadioEther_90To110MHz_20Hz),
                new PresetItem("Тільки шум (90–110 МГц, 20 Гц)", SpectrumGeneratorPresets.NoiseOnly_90To110MHz_20Hz),
                new PresetItem("Один несучий сигнал (90–110 МГц, 20 Гц)", SpectrumGeneratorPresets.SingleCarrier_90To110MHz_20Hz),
                new PresetItem("Дрейфуючий сигнал (90–110 МГц, 20 Гц)", SpectrumGeneratorPresets.DriftingCarrier_90To110MHz_20Hz),
                new PresetItem("Широкосмуговий блок (90–110 МГц, 20 Гц)", SpectrumGeneratorPresets.WidebandBlock_90To110MHz_20Hz),
                new PresetItem("Стрес-тест (90–110 МГц, 20 Гц)", SpectrumGeneratorPresets.StressTest_90To110MHz_20Hz),
            };

            timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Tick += OnTimerTick;

            SelectedPreset = Presets[0];
        }

        private void Start()
        {
            IsRunning = true;
            timer.Start();
        }

        private void Stop()
        {
            IsRunning = false;
            timer.Stop();
        }

        private bool CanStart() => !IsRunning;

        private bool CanStop() => IsRunning;

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (!IsRunning)
            {
                return;
            }

            CurrentFrame = generator.CreateNextFrame();
        }

        private void ApplyPreset(PresetItem preset)
        {
            var wasRunning = IsRunning;

            if (wasRunning)
            {
                Stop();
            }

            generationOptions = preset.Factory();
            generator = new SpectrumGenerator(generationOptions);

            timerInterval = TimeSpan.FromSeconds(1.0 / generationOptions.UpdateRateHz);
            timer.Interval = timerInterval;

            CurrentFrame = new SpectrumFrame(
                generationOptions.FrequencyStartMhz,
                generationOptions.FrequencyEndMhz,
                generationOptions.MinimumPowerDbm,
                generationOptions.MaximumPowerDbm,
                new double[generationOptions.BinCount]);

            if (wasRunning)
            {
                Start();
            }
        }
    }
}
