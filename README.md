

<div align="center">

# 🐀 ratgoy

**Форк Space Station 14 с уникальными механиками и контентом**

[![Discord](https://img.shields.io/discord/1280958916691361842?style=for-the-badge&logo=discord&logoColor=white&label=Discord&color=%237289da)](https://discord.gg/f7ZCyH4Qf8)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?&style=for-the-badge)](https://dotnet.microsoft.com/)

</div>

---

## 📋 О проекте

**Ratgore** — это форк [Space Station 14](https://github.com/space-wizards/space-station-14), космического симулятора на движке Robust Toolbox. Проект добавляет уникальные механики, контент и улучшения геймплея с фокусом на атмосферу и уникальный игровой опыт.

## 🚀 Быстрый старт

### Требования

- **Git** — [скачать](https://git-scm.com/downloads)
- **.NET SDK 10.0 или выше** — [скачать](https://dotnet.microsoft.com/download/dotnet/10.0)

### 🍃 Windows

```
# 1. Клонируйте репозиторий
git clone https://github.com/ss14-art/ratgoy.git
cd ratgore

# 2. Загрузите движок
git submodule update --init --recursive

# 3. Соберите проект
Scripts\bat\buildAllRelease.bat

# 4. Запустите клиент и сервер
Scripts\bat\runQuickAll.bat
```

**Готово!** Подключитесь к **localhost** в клиенте и играйте

### 🐧 Linux / macOS

```
# 1. Клонируйте репозиторий
git clone https://github.com/ss14-art/ratgoy.git
cd ratgore

# 2. Загрузите движок
git submodule update --init --recursive

# 3. Соберите проект
chmod +x Scripts/sh/buildAllRelease.sh
Scripts/sh/buildAllRelease.sh

# 4. Запустите клиент и сервер
chmod +x Scripts/sh/runQuickAll.sh
Scripts/sh/runQuickAll.sh
```

**Готово!** Подключитесь к **localhost** в клиенте и играйте

## 📜 Лицензия

Код проекта распространяется под лицензией **SS14-ART PROJECT LICENSE**. Ассеты имеют различные лицензии (в основном CC-BY-SA 3.0).

Подробную информацию о лицензиях смотрите в файле [LICENSE.TXT](./LICENSE.TXT).


