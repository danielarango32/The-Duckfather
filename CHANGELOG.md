# Registro de cambios

Cambios aplicados sobre el proyecto a raíz de la auditoría del 20/08/2026.
Cada entrada referencia el identificador del hallazgo en el informe.

**Informe completo:** https://claude.ai/code/artifact/18f766d1-9669-4a32-a4a5-fd685509a172

## Cómo se verifica cada cambio

Sin abrir el editor de Unity:

- **Código:** se compila `Assembly-CSharp` con el Roslyn que trae Unity
  2022.3.19f1, usando las 254 referencias del `Assembly-CSharp.csproj` más los
  ensamblados con `.asmdef` de `Library/ScriptAssemblies` (Photon, TMPro,
  Cinemachine…). Criterio de aceptación: 0 errores y 0 avisos.
- **Escenas:** se parsea el YAML con un lector real (no expresiones regulares)
  y se comprueban nodo a nodo las referencias tocadas: método, target,
  argumento, jerarquía padre-hijo y posición.

Lo que **no** cubre esta verificación: el comportamiento en red. Todo lo que
toca RPC o flujo de sala necesita una prueba en el editor con dos clientes.

---

## 21/08/2026

### F-23 · Los patos remotos saltaban con tu input

`Dani/AnimatorController.cs`

`PlayerSetUp.IsNotLocalPlayer()` deja el `AnimatorController` habilitado también
en los jugadores remotos, y hace falta que sea así: es lo que mueve su animator.
Pero su `Update()` leía `Input.GetButtonDown("Jump")` — el input **local** — con
dos consecuencias:

1. todos los patos remotos saltaban a la vez que tú;
2. la rama `else` escribía `IsJumping = false` en cada frame, pisando el valor
   que acababa de dejar el RPC `SyncJumpState` y dejando ese RPC sin ningún
   efecto.

- Nuevo `UpdateOwnJumpState()`, que sale temprano si el `PhotonView` no es
  nuestro. En los remotos el parámetro `IsJumping` pasa a escribirlo únicamente
  el RPC.
- `MotionX` y `MotionY` se quedan como estaban, y siguen funcionando en ambos
  casos: `playerMovement.x/z` los alimenta el input local en el dueño y el RPC
  `SyncMovement` en los remotos. Se verificó en el código de PUN que los RPC sí
  llegan a componentes deshabilitados — `ExecuteRpc` recorre
  `RpcMonoBehaviours` sin filtrar por `enabled` —, que es lo que hace que eso
  funcione aunque `PlayerMovement` esté desactivado en los remotos.
- El literal `"IsJumping"` pasa a constante: lo escriben dos caminos distintos y
  conviene que sea literalmente el mismo parámetro.

Con esto queda cerrado el problema del salto en red: **F-22** arregló el envío
del RPC (solo en el flanco, dos por salto) y **F-23** hace que ese RPC llegue a
mandar de verdad en el receptor.

## 20/08/2026

### F-13, F-17, F-18, F-24, F-43, F-50 · Borrado de sistemas reemplazados

30 assets borrados con sus `.meta`: 8 scripts, 10 prefabs huérfanos y 12
escenas fuera del build.

- **Scripts:** `Room and lobby manager.cs` y `Multiplayer/RoomManager.cs` (los
  dos gestores de sala anteriores a `Launcher`), `GunSpawn.cs` (el spawner roto
  de F-24, duplicado de `SpawnGun`), `DisparoRaycast.cs` (disparo sin red),
  `Recoger_Arma.cs` (NRE por frame, cuerpo vacío), `ItemInfo.cs` (77 campos sin
  usar), `win.cs` y `MovimientoJugador.cs`.
- **Escenas:** `Scenes/DANI/{Offline,Online}`, `Scenes/Elite/{Offline,Online}`,
  `Scenes/TEO/{Offline,Online}` y las 6 de `Scenes/Rolix/scene no funciona/`.
- **Prefabs:** `Prefabs/Offline_Teo.prefab`, los 6 de `Scenes/DANI/prefabas/` y
  los 3 de `Scenes/Elite/Prefabs_PowerUps/`.

**Verificación previa al borrado.** Se extrajo el GUID de cada candidato y se
buscó en los 1.580 assets del proyecto. Ninguno aparece en las tres escenas del
build ni en ninguno de los 28 prefabs de
`Photon/PhotonUnityNetworking/Resources/`, que es donde viven de verdad los
objetos que instancia `PhotonNetwork.Instantiate` por nombre. Los prefabs de
`Scenes/DANI/prefabas/` eran copias huérfanas: se comprobó que `Pato 1.prefab`
referencia las de `Resources`, no esas.

**Corrección importante sobre el plan inicial.** La propuesta era borrar las
carpetas `DANI/`, `Elite/` y `TEO/` enteras. El análisis de dependencias lo
desmintió: **46 de sus 122 ficheros son carga viva**. `DANI/Animaciones/` tiene
los animator controllers y el FBX del pato que usan `Pato 1/2/3` y `Player*`;
`Elite/Audio/` tiene 26 sonidos que usan esos mismos prefabs y hasta
`Online 2.unity`. Borrar por carpeta habría roto el juego. Se borraron solo los
ficheros con cero referencias externas.

**Deliberadamente NO borrado:** los 46 ficheros de arte y audio sin referenciar
que quedan en esas carpetas. Técnicamente son huérfanos, pero son fuentes de
los compañeros (FBX, texturas, mp3) y esa decisión es suya, no una consecuencia
técnica del borrado de código.

### F-05 · La lista de salas se reconstruía desde el delta

`UI/newScript/Launcher.cs`

`OnRoomListUpdate` recibe de Photon **solo los cambios** desde la última
actualización, no el listado completo. El código destruía todos los items y
reinstanciaba únicamente los del delta, así que las salas que seguían
existiendo pero no habían cambiado en esa actualización desaparecían del menú
«find room».

- Nuevo `Dictionary<string, RoomInfo> cachedRoomList` con el acumulado.
  `MergeRoomListDelta()` aplica el delta: alta o actualización por nombre de
  sala, y baja cuando llega `RemovedFromList`.
- `RedrawRoomList()` redibuja la UI desde el acumulado. Lleva su propia lista
  `spawnedRoomListItems` en lugar de recorrer los hijos de `roomListContent`:
  `Destroy` es diferido hasta el final del frame, así que recorrer hijos podía
  arrastrarse los items recién creados si llegaban dos actualizaciones en el
  mismo frame.
- `ClearRoomList()` vacía el acumulado y la UI. Se llama en `OnJoinedLobby`
  (Photon reenvía el listado completo justo después de entrar; si quedara algo
  del lobby anterior se mezclarían salas ya inexistentes), en el nuevo
  `OnLeftLobby` y en `OnDisconnected`.

### F-25, F-26 · Objetos de red que no se destruían o se quedaban huérfanos

`Multiplayer/NetworkCleanup.cs` (nuevo), `Player/VFX/VFX_Destroyer.cs`,
`Emanuel_Scrips/Bala.cs`, `Player/Armas/WeaponCollision.cs`

`VFX_Destroyer` comparaba `tiempo == particulas.main.duration` con `==`, cosa
que en coma flotante prácticamente nunca se cumple: los efectos **no se
destruían nunca**. Como cada disparo crea dos VFX con `PhotonNetwork.Instantiate`,
se acumulaban en todos los clientes durante toda la partida. Y donde sí se
destruía algo (`Bala`, `WeaponCollision`) se usaba `Object.Destroy`, que borra
solo en el cliente que llama y deja el objeto huérfano en el resto.

- **Nuevo `NetworkCleanup.Remove(GameObject)`**: decide la vía correcta según
  el objeto. Si tiene `PhotonView` con `InstantiationId != 0` vino de
  `PhotonNetwork.Instantiate` y solo su dueño puede borrarlo, con
  `PhotonNetwork.Destroy`; si no, `Object.Destroy` local. Evita repetir esas
  seis líneas en tres sitios.
- **`VFX_Destroyer` reescrito**: acumula `Time.deltaTime` y se borra al llegar
  a `duration + startLifetime`, con `vidaPorDefecto` como respaldo si el
  sistema de partículas no define duración. El `ParticleSystem` se resuelve una
  vez en `Awake()` en lugar de dos veces por frame. La bandera
  `destruccionPedida` evita pedir el borrado más de una vez.
- **`Bala.Delay()`** pasa por `NetworkCleanup.Remove()`.
- **`WeaponCollision`** pasa por `NetworkCleanup.Remove()`.

**Además, dos defectos encontrados al tocar `Bala.Explode()`** (no estaban en el
informe): `Invoke("DelayExplosion", 5.0f)` no llegaba a ejecutarse nunca, porque
`Invoke("Delay", 0.05f)` destruía la bala 0,05 s antes y eso cancela los Invoke
pendientes del objeto — el efecto de explosión no se borraba jamás. Y cuando sí
hubiera corrido, `Destroy(explosion)` apuntaba al **prefab de referencia**, no
al clon recién creado. Ahora el efecto se limpia con `Destroy(efecto, 5f)`, que
va asociado al propio efecto y sobrevive a la destrucción de la bala;
`DelayExplosion()` desaparece.

**Queda pendiente:** la duplicación de armas **no se arregla con esto**. La
causa real es que `SpawnGun` las crea con `Instantiate` local, así que cada
cliente tiene su propia copia y `NetworkCleanup.Remove()` degrada
—correctamente— a un borrado local. El arma seguirá pudiendo recogerse dos
veces hasta que el spawner pase a `PhotonNetwork.Instantiate` con el prefab en
una carpeta `Resources`.

Esto no lo cubrió el bloque 8: allí se borró `GunSpawn` (la implementación
rota), pero `SpawnGun` —la que quedó viva— sigue instanciando en local. Es
trabajo pendiente sin bloque asignado, y `NetworkCleanup.Remove()` ya deja el
camino listo para cuando se haga.

### F-31, F-32 · Victoria automática al crear la partida y corrutinas por frame

`UI/newScript/PlayerManager.cs`

`WinningConditions()` se llama desde `Update()` y tenía dos problemas
encadenados. `PhotonNetwork.PlayerList.Length == 1` se cumple en el primer frame
al crear una partida en solitario, así que salía el cartel de victoria y a los
4 segundos `EndGame()` te expulsaba de tu propia sala. Y como no había ninguna
bandera de partida terminada, mientras la condición siguiera cumpliéndose se
arrancaba una corrutina nueva **por frame** (≈60/s), cada una llamando a
`EndGame()`.

- Nueva bandera `matchEnded`: `WinningConditions()` sale temprano en cuanto la
  partida ha terminado, así que la corrutina de victoria o derrota se arranca
  una sola vez.
- Nueva bandera `matchHasHadRivals`: la victoria por quedarse solo en la sala
  solo cuenta si en algún momento hubo al menos `MinimumPlayersForWin` (2)
  jugadores. Crear una partida y esperar ya no la dispara.
- La comprobación de `PV.IsMine` sube al principio del método, en lugar de
  repetirse en cada condición.
- Victoria y derrota son ahora mutuamente excluyentes: antes las dos ramas
  podían dispararse en el mismo frame.

**Queda pendiente (F-33):** la derrota sigue comparando contra el literal
`Death == 5` en vez de contra el campo serializado `deathsTarget`, que se
declara y no se lee nunca. Es una palabra dentro del método que se acaba de
reescribir, pero es su propio hallazgo y se deja trazable.

### F-21 · Bucle infinito de RPC al cambiar de arma

`Player/ThePlayer/ShootinController.cs`

`SelectorDeArma()` terminaba emitiendo `SyncWeaponChange` a `RpcTarget.Others`,
y ese RPC volvía a llamar a `SelectorDeArma()`, que emitía el RPC otra vez:
A → B → A → B sin condición de parada. Cada vuelta arrastraba además un
`PlayGrabSFX` a `RpcTarget.All`, así que también era una tormenta de audio.

- Se separa la aplicación local de la difusión por red:
  - `SelectorDeArma(float)` es el punto de entrada del jugador propio: aplica,
    suena y propaga. Sale temprano si `!photonView.IsMine`, porque el trigger
    de recogida salta en todos los clientes y solo el dueño debe decidir.
  - `ApplyWeapon(float)` (nuevo, privado) aplica el arma **sin tocar la red**.
  - El `[PunRPC] SyncWeaponChange` llama a `ApplyWeapon`, no a
    `SelectorDeArma`. Ahí es donde se cierra el bucle.
- `photonView` y `playerPhotonSoundManager` se resuelven en `Awake()` en lugar
  de `Start()`: la nueva guarda `photonView.IsMine` puede evaluarse desde
  `WeaponCollision.OnTriggerEnter` antes de que `Start()` haya corrido.

**Queda pendiente:** el RPC va a `RpcTarget.Others` sin buffer, así que quien
entre a la sala después no ve el arma que ya lleva cada jugador. Era igual
antes; se arregla con `OthersBuffered` o sincronizando el arma en las
propiedades del jugador.

### F-22 · RPCs de movimiento y salto en cada frame

`Player/ThePlayer/PlayerMovement.cs`

`Movimiento()` emitía `SyncMovement` en cada `Update` aunque el input no
hubiera cambiado, y la rama `else` de `Saltar()` emitía `SyncJumpState(false)`
60 veces por segundo. Con 8 jugadores salían del orden de 1440 RPC/s de ruido.

- `SyncMovementIfChanged()` envía solo cuando el input cambia por encima de
  `MovementSyncThreshold` (0,05) y como mucho una vez cada
  `MovementSyncInterval` (0,1 s → 10 envíos/s).
- `MovementNeedsSync()` fuerza el envío en las transiciones parado ↔ en
  movimiento. Sin eso, el último valor enviado podía quedarse en un residuo
  dentro del umbral y el muñeco remoto seguiría andando en el sitio.
- `SyncJumpStateIfChanged(bool)` envía solo en el flanco: dos RPC por salto en
  lugar de un flujo continuo.
- Ambos comprueban `photonView.IsMine` antes de emitir, de modo que el volumen
  de RPC queda acotado al dueño del objeto.

**Queda pendiente:** el salto sigue sin verse bien en los jugadores remotos,
pero por **F-23**, no por esto: `AnimatorController.Update` lee
`Input.GetButtonDown("Jump")` en todos los clientes y pisa cada frame el valor
que acaba de escribir el RPC. Arreglar F-21/F-22 baja el tráfico; la animación
remota no se arregla hasta F-23.

### F-06, F-07 · Salida de partida duplicada y sin reconexión posible

`UI/newScript/RoomManageNew.cs`, `UI/Pause.cs`, `UI/newScript/PlayerManager.cs`,
`UI/newScript/Launcher.cs`

`Pause.BackToLobby()` y `PlayerManager.EndGame()` contenían el mismo bloque de
20 líneas copiado: 14 `PhotonNetwork.Destroy(GameObject.Find("..."))` sobre
objetos inexistentes (cada uno un `Debug.LogError`), un `Disconnect()` y un
`LoadLevel()` justo después. Al cargar el menú, `ConnectUsingSettings()`
devolvía `false` porque el peer aún no estaba `Disconnected`
(`PhotonNetwork.cs:1135-1139`), el valor de retorno se ignoraba y el jugador se
quedaba en «loading» sin poder volver a crear partida.

- Nuevo `RoomManagerNew.ExitMatch()`: destruye los objetos de red del jugador
  local con un único `DestroyPlayerObjects`, hace `LeaveRoom()` y espera a
  `OnLeftRoom` para cargar el menú con `SceneManager.LoadScene`. **Ya no se
  desconecta**: seguir en el Master Server es lo que permite volver a crear
  partida al instante.
- El flag `isLeavingMatch` distingue la salida de partida de un `LeaveRoom`
  normal desde el lobby, que sigue gestionando `Launcher`.
- `Pause.BackToLobby()` y `PlayerManager.EndGame()` quedan en una línea cada uno.
- `Launcher.Start()` cubre los tres estados posibles al arrancar: ya en el
  lobby, conectado pero aún no listo (corrutina `RequestLobbyWhenReady`, que
  elimina la carrera entre el callback de Photon y el registro del nuevo
  `Launcher` al cargar la escena) o desconectado. El flag `hasRequestedLobby`
  evita el doble `JoinLobby()`.

### F-03, F-04 · El menú de crear partida fallaba en silencio

`UI/newScript/Launcher.cs`

`CreateRoom()` hacía `return` sin mensaje si el nombre estaba vacío, y no
existían `OnJoinRoomFailed` ni `OnDisconnected`, así que cualquier fallo al
entrar en una sala dejaba al jugador en «loading» indefinidamente.

- `CreateRoom()` recorta el nombre y rechaza el vacío, el que pasa de
  `MaxRoomNameLength` y el caso de no estar conectado, cada uno con su mensaje.
- Nuevo helper `ShowError(string)`, que usan también los tres callbacks de fallo.
- `OnJoinRoomFailed` muestra el motivo que devuelve Photon.
- `OnDisconnected` reintenta la conexión una vez (`MaxReconnectAttempts`) y, si
  esa también falla, muestra un mensaje terminal en lugar de reintentar en bucle.
- Las mismas validaciones de conexión cubren `JoinRoom()`.

### F-01, F-02 · Callejones sin salida en el flujo de crear partida

`Scenes/Rolix/Online 2.unity`

El botón «Volver» de la pantalla de error estaba cableado a
`MenuManager.CloseMenu(TitleMenu)`, que cerraba un menú **ya cerrado**: el
`ErrorMenu` se quedaba abierto y ningún otro menú se abría nunca. Y la pantalla
`Creat Room Screen` no tenía ningún botón de regreso, así que la única salida
era crear una sala con éxito.

- El `OnClick` de «Volver» pasa a `MenuManager.OpenMenu(TitleMenu)` — la
  sobrecarga por objeto, que cierra lo abierto antes de abrir. Un solo campo del
  YAML; se conservan `m_Mode: 2` y el argumento.
- Se añade un botón «Volver» a `Creat Room Screen`, copia del que ya existe en
  el `ErrorMenu` (mismo sprite, mismos colores de transición), cableado también
  a `MenuManager.OpenMenu(TitleMenu)`. Anclado abajo-centro en `y: 150`, con
  41 px de holgura respecto al botón «crear».

---

## Hallazgos que siguen abiertos

**El orden de arreglo acordado (bloques 1 a 8) está completo:** 21 hallazgos
corregidos y 2 parciales (F-16 y F-50). Quedan 29 sin prioridad asignada: sobre todo los de rendimiento y código muerto
del bloque E del informe, más los de convenciones y repositorio.

El siguiente natural, por quedar a un paso de un cambio ya hecho:

| Hallazgo | Qué | Dónde |
|---|---|---|
| F-33 | Usar `deathsTarget` en vez del literal `5`, y `>=` en vez de `==` | `PlayerManager.WinningConditions()` |

Sigue abierto también **F-27** (`Bala.Explode()` sin guarda de reentrada) y
**F-28** (daño de explosión aplicado en local), que se dejaron intactos al
tocar `Bala.cs` para F-25 por ser hallazgos propios.
