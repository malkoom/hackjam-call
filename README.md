# HackJam12

Juego local para dos jugadores creado con Unity. Los jugadores compiten en una
serie de minijuegos para conseguir piezas y montar sus vehículos; al terminar
la ronda, el juego muestra el resultado final.

## Características

- Competición local para dos personas con teclado compartido.
- Secuencia de minijuegos seleccionados por el `GameManager`.
- Garaje intermedio donde se entregan las piezas conseguidas.
- Minijuegos de sumo, caracoles, torres, semáforo, gasolina, Squid Game y
  policía.
- Transiciones visuales, UI y audio integrados con FMOD.

## Requisitos

- [Unity Hub](https://unity.com/download) y **Unity 6000.6.0f1**.
- Un teclado para los dos jugadores.

El proyecto utiliza Universal Render Pipeline (URP), el Input System de Unity,
Splines y FMOD. Las dependencias del proyecto están declaradas en
`Packages/manifest.json` y Unity las restaurará al abrirlo.

## Abrir y ejecutar

1. Clona el repositorio:

   ```bash
   git clone https://github.com/malkoom/hackjam-call.git
   ```

2. En Unity Hub, selecciona **Add** y elige la carpeta raíz del repositorio.
3. Ábrelo con Unity `6000.6.0f1` y espera a que termine la importación de
   recursos.
4. Abre `Assets/Scenes/IntroScene.unity` y pulsa **Play**.

Para generar una versión ejecutable, usa **File > Build Profiles** en Unity.
Las escenas necesarias ya están incluidas en los ajustes de compilación.

## Controles

La mayoría de minijuegos usan esta distribución:

| Jugador 1 | Jugador 2 |
| --- | --- |
| `W`, `A`, `S`, `D` | Flechas de dirección |

Las instrucciones concretas aparecen durante cada minijuego. Por ejemplo,
Sumo usa `W` y `↑` para empujar; en Torres, `W`/`↑` lanzan y `A`/`D` y `←`/`→`
sirven para equilibrarse.

## Flujo de juego

```text
Intro → Minijuego → Garaje (entrega de pieza) → siguiente minijuego → Outro
```

`GameManager` conserva el estado entre escenas, asigna una pieza a cada
minijuego, entrega la pieza al ganador y termina la partida cuando no quedan
minijuegos disponibles.

## Estructura

```text
Assets/
├── Scenes/                 Escenas de introducción, garaje, cierre y juego
│   └── Minigames/          Escenas de cada minijuego
├── Scripts/
│   ├── Managers/           Estado y flujo global de la partida
│   ├── Minigames/          Lógica común y específica de los minijuegos
│   └── UI/                 Interfaz y transiciones
├── Prefabs/                Prefabs compartidos y de minijuegos
└── FMOD/                   Proyecto y bancos de audio
Packages/                   Dependencias de Unity
ProjectSettings/            Configuración del proyecto
```

## Desarrollo

Para añadir un minijuego:

1. Crea su escena dentro de `Assets/Scenes/Minigames/`.
2. Implementa la lógica heredando de `AMiniGame`.
3. Añade la escena a **File > Build Profiles > Scene List**.
4. Configura la pieza, la escena y su descripción en `GameManager`.
5. Al determinar el ganador, llama a `NotifyWinner(1)` o `NotifyWinner(2)` y,
   al finalizar, a `ReturnToMiddleScene()`.

## Tecnología

- Unity 6.0 (`6000.6.0f1`)
- Universal Render Pipeline
- Unity Input System
- Unity Splines
- FMOD Studio
