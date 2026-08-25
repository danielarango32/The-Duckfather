# The DuckFather

Proyecto de **Taller 7** desarrollado por **Ultra Game Studio**: un Battle Royale
individual en 3D, ambientado en una guerra urbana entre mafias de patos.

Este README resume el contenido del pitch del proyecto
([presentación en Canva](https://canva.link/q5krnrqsqj7uc43), 12 diapositivas).

---

## Por qué este repositorio sigue cambiando

El proyecto se entregó y calificó como Taller 7; como pasa con la mayoría de
entregas de curso, el equipo se disolvió ahí y el código quedó congelado con
los bugs típicos de una entrega contrarreloj: sistemas a medio terminar (el
escudo estaba declarado pero nunca restaba daño), condiciones de carrera en
red sin detectar (colisiones de `PhotonView` ID, tormentas de RPC) y assets
muertos sin limpiar.

Desde el 20/08/2026 estoy retomando el proyecto por mi cuenta como ejercicio
de refactor: lo trato como si fuera a salir a producción, no como una tarea ya
entregada. El proceso es el de un pase de hardening real:

1. **Auditoría completa** con una rúbrica de código limpio: 52 hallazgos
   catalogados por severidad e impacto.
2. **Arreglo por bloques**, de mayor a menor coste/beneficio — no lo primero
   que aparece.
3. **Verificación sin abrir el editor de Unity** cuando es posible: compilar
   `Assembly-CSharp` con el mismo Roslyn que trae el Editor, y parsear escenas
   y prefabs con un lector YAML real en vez de expresiones regulares, para no
   depender de inspección visual.
4. **Registro exhaustivo**: cada cambio queda documentado con su causa raíz,
   no solo el síntoma, en [`CHANGELOG.md`](CHANGELOG.md).

Algunos ejemplos de lo que salió de esta auditoría — no son cambios
cosméticos:

- Una tormenta de ~1.440 RPC/segundo con 8 jugadores (un RPC de movimiento y
  otro de salto en cada `Update()`, sin importar si el input había cambiado).
- Un bucle infinito de RPC al cambiar de arma: A llama a B, B vuelve a llamar
  a A, sin condición de parada.
- Objetos de red que nunca se destruían porque el código comparaba floats con
  `==` en vez de un umbral — se acumulaban en todos los clientes durante toda
  la partida.
- Una colisión de `PhotonView ID` que rompía la sala al jugar una segunda
  partida, diagnosticada cruzando la consola del juego con el código fuente
  real de Photon PUN2.
- Vida y escudo con dos indicadores de UI que se desincronizaban entre sí, un
  escudo que nunca restaba daño, y un dash que duraba menos de un frame.

**Este refactor se está haciendo con ayuda de Claude** (el asistente de código
de Anthropic), usado como compañero de programación: lee el SDK de Photon para
encontrar la causa raíz rápido, propone y aplica el fix, y compila el proyecto
para verificar antes de cada commit. Las decisiones de diseño, la revisión de
cada cambio y las pruebas en el editor con varios clientes las hago yo — la IA
acelera el diagnóstico y la escritura, no reemplaza el criterio.

### Cronología del refactor

| Fecha | Qué cambió |
|---|---|
| 20/08/2026 | Auditoría completa (52 hallazgos) y primeros 4 bloques de arreglos: flujo de creación de sala con callejones sin salida y fallos silenciosos, salida de partida duplicada sin reconexión posible, tormenta de RPC de movimiento y salto, bucle infinito de RPC al cambiar de arma, victoria automática al crear partida en solitario, objetos de red huérfanos, limpieza de 30 assets muertos (8 scripts, 10 prefabs, 12 escenas). |
| 21/08/2026 | Patos remotos saltando con el input de otro jugador; congelamiento total del cliente por doble clic en "empezar partida"; colisión de `PhotonView ID` que impedía jugar una segunda partida sin reiniciar la app. |
| 23/08/2026 | Barra de vida y escudo reparadas y unificadas, escudo funcional (absorbe daño y regenera), daño de la bazuca que nunca llegaba en red, dash arreglado (duraba menos de un frame). Más tarde el mismo día: flash de impacto en red y VFX de muerte (explosión de plumas) antes del respawn, reemplazando el respawn instantáneo sin transición. |
| 24/08/2026 | Diagnóstico y arreglo del flash de impacto invisible para otros jugadores: el material del pato no tenía la emisión activada, así que el color nunca se veía por encima de la iluminación de la escena. |

El detalle técnico completo — causa raíz, código exacto, cómo se verificó
cada cambio — está en [`CHANGELOG.md`](CHANGELOG.md).

---

## 1. El estudio

**ULTRA — Game Studio.** La presentación abre con el logotipo del estudio
(una silueta blanca sobre fondo negro) y su firma tipográfica.

## 2. El equipo

Nueve integrantes, con los roles tal como aparecen en la diapositiva *Team*:

| Integrante | Rol |
|---|---|
| Sara Londoño | Animator |
| David Marín | 3D Modeler · Animator |
| Emanuel Perez | Developer |
| Mateo Arango (External help) | Developer | 
| Felipe Aguilar | Developer and Rigger |
| Daniel Arango | Project Manager, Programers |
| Andres Delgado | Art Director |
| Michael Munera | 3D Modeler · Animator |
| Juan Pablo Cardona | 3D Modeler · Animator |
| Emilio Jimenez | Animation Director |


## 3. Servicios

El estudio se presenta con dos líneas de servicio:

- **Desarrollo de videojuegos**
- **Producción de contenido digital**

## 4. El proyecto: *The DuckFather*

La diapositiva de proyecto da paso al título de la propuesta: **The DuckFather**.

### Descripción

> ¡Bienvenido a 'The DuckFather', el campo de batalla definitivo impulsado por
> adrenalina donde la estrategia astuta se encuentra con el poder de fuego
> implacable! En esta experiencia individual de Battle Royale, los jugadores son
> lanzados al corazón de una zona de guerra urbana dominada por mafias rivales
> que luchan por el control.

Los tres pilares que se leen en esa descripción son:

- **Battle Royale individual** — todos contra todos, sin equipos.
- **Estrategia + poder de fuego** — el combate premia tanto posicionarse bien
  como disparar bien.
- **Zona de guerra urbana** — el mapa es una ciudad disputada por mafias rivales.

## 5. Mood board — "Arte Universo"

El moodboard divide la referencia visual en dos columnas:

- **Arte escenarios** — capturas de *Worms Rumble* y dioramas isométricos de
  edificios urbanos nocturnos con iluminación de neón. Marcan el tono del mapa:
  ciudad estilizada, colorida, legible desde cámara alejada.
- **Arte personajes** — patos y animales caricaturescos armados (*Worms Rumble*,
  el pato con pistola), junto a referencias directas del cine de mafia:
  *Peaky Blinders*, *Scarface* y *El Padrino*, además de un modelo 3D de un pato
  con sombrero y traje negro, y un arma tipo uzi.

La mezcla define el concepto: **estética cartoon de shooter multijugador +
iconografía clásica del cine de gángsters.** La diapositiva cierra con dos
paletas de color de referencia (grises, rojo y beige/amarillo).

## 6. Concept Art

Hojas de diseño del personaje protagonista, todas rotuladas *The DuckFather*:

- Estudios de silueta y pose del pato base, y variaciones con un arma larga
  sostenida en el pico.
- Bocetos rápidos a color (naranja, azul, morado) explorando proporciones.
- Versiones vestidas: pato con sombrero fedora, gabardina, reloj de pulsera y
  puro en el pico.
- Una vuelta de personaje (turnaround) en gris con distintos sombreros y trajes.
- Una versión final a color: pato amarillo con chaleco negro, sombrero y puro.

## 7. Mecánicas

La diapositiva de mecánicas lista dos puntos (la numeración salta del 1 al 3 en
la presentación original):

- **1 – Se usarán power ups**
- **3 – Conquistar zona**
---

## Estado del repositorio

El pitch describe la propuesta; este repositorio contiene la implementación en
Unity **2022.3.19f1**, con multijugador sobre **Photon PUN 2**.

Los 44 scripts propios que quedan tras la limpieza de código muerto (eran 51
en la entrega original; ver [Cronología del refactor](#cronología-del-refactor))
viven en `The Duckfather/Assets/Scripts/` y cubren, entre otros:

- **Red y salas** — `Launcher`, `RoomManageNew`, `RoomListItem`,
  `PlayerListItem`, `SpawnManager`, `PlayerManager`, `NetworkCleanup`.
- **Jugador** — `PlayerMovement` (movimiento y dash), `MouseLook`, `PlayerSetUp`,
  `AnimatorController`, `LifeManager` (vida, escudo, flash de golpe y muerte).
- **Combate** — `ShootinController` (raycast y proyectiles), `Bala`,
  `PlayerWeaponManager`, `WeaponCollision`, `SpawnGun`, `VFX_Destroyer`.
- **Puntería** — máquina de estados `AimBaseState` / `AimState` / `HipsFireState`
  sobre Cinemachine.
- **Power-ups** — `PowerUp`, `PowerUpEffect`, `SpeedBuff`, `JumpBuff`
  (la mecánica 1 del pitch).
- **Partida y UI** — `Timer`, `ScoreBoard`, `ScoreBoardItem`, `Botones`,
  `Creditos`, `Logo`, `Pause`.
- **Audio** — `PlayerPhotonSoundManager`, `WavUtility`.

La mecánica **"Conquistar zona"** no tiene todavía un script equivalente en
`The Duckfather/Assets/Scripts/`.

Un grafo navegable del código está disponible en `graphify-out/graph.html`
(informe en `graphify-out/GRAPH_REPORT.md`), y se mantiene actualizado con
cada tanda de cambios del refactor.
