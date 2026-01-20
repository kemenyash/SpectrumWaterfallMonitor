# SpectrumWaterfallMonitor.Controls

Модуль WPF-контролів для візуалізації спектра

## Структура
```
Controls
├── Spectrum
│   └── Infrastructure
└── Waterfall
    └── Infrastructure
```

## Spectrum
- SpectrumControl - безпосередній контроль спектра

Infrastructure:
- SpectrumSampler - зведення бінів спектра до ширини контролу (щоб забезпечити адпативність)
- SpectrumSmoother - згладжування спектра (щоб не було різких стрибків)
- SpectrumGeometryBuilder - перетворення спектра dBm  у реальну геометрію
- SpectrumGridRenderer - малювання сітки для спектра
- SpectrumStyle - стилі спектра

## Waterfall
- WaterfallControl - безпосередній контрол водоспаду

Infrastructure:
- SpectrumRowAccumulator - формування рядків спектра для водоспаду
- WaterfallBitmapBuffer - кільцевий буфер
- WaterfallRowWriter - записує готовий dBm рядок спектра і записує в піксельний рядок
- WaterfallRenderer - рендеринг водоспаду враховуючи буфер, зум, перетягування
- WaterfallViewport - зум водоспаду
- WaterfallDragController - перетягування по водоспаду
- GradientLookupFactory - створює відповідні градієнти для водоспаду
