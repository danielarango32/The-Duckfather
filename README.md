# The DuckFather

Proyecto de **Taller 7** desarrollado por **Ultra Game Studio**: un Battle Royale
individual en 3D, ambientado en una guerra urbana entre mafias de patos.

Este README resume el contenido del pitch del proyecto
([presentación en Canva](https://canva.link/q5krnrqsqj7uc43), 12 diapositivas).

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

Los 51 scripts propios viven en `The Duckfather/Assets/Scripts/` y cubren, entre
otros:

- **Red y salas** — `Launcher`, `RoomManager`, `RoomListItem`, `PlayerListItem`,
  `SpawnManager`, `PlayerManager`.
- **Jugador** — `PlayerMovement` (movimiento y dash), `MouseLook`, `PlayerSetUp`,
  `AnimatorController`, `LifeManager` (vida y escudo).
- **Combate** — `ShootinController`, `DisparoRaycast`, `Bala`,
  `PlayerWeaponManager`, `Recoger_Arma`, `SpawnGun`, `GunSpawn`.
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
(informe en `graphify-out/GRAPH_REPORT.md`).
