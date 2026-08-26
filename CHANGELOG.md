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

## 25/08/2026 (8)

### El scoreboard publico arrancaba con las muertes de la partida anterior

`UI/newScript/Launcher.cs`, `UI/newScript/RoomManageNew.cs`

Reportado con captura: el HUD propio mostraba "Muertes: 1" pero la tabla
publica (Tab) mostraba "2" para el mismo jugador, en la misma partida.

**Causa, confirmada en el codigo fuente real de Photon Realtime
(`LoadbalancingPeer.cs:1973`, `LoadBalancingClient.cs:2109-2119`), no por
especulacion:**

- `broadcastPropsChangeToAll` vale `true` por defecto, y `Launcher.CreateRoom()`
  llama `PhotonNetwork.CreateRoom(roomName)` sin `RoomOptions` — se queda en
  ese valor por defecto.
- Con `BroadcastPropsChangeToAll = true`, `SetCustomProperties()` **no
  actualiza el cache local al instante**: `OpSetPropertiesOfActor()` solo hace
  el update optimista en el propio cliente cuando esa bandera es `false`; si
  es `true` (este caso), incluso el que hizo el cambio tiene que esperar a
  que el servidor se lo confirme de vuelta.
- El reset de "deaths"/"Kills" (agregado en la entrada (4) de este mismo dia)
  vivia en `RoomManagerNew.OnSceneLoaded()`, que corre pegado, en el mismo
  frame en que carga la escena de partida — y `ScoreBoard.Start()` (que crea
  cada `ScoreBoardItem` y hace su **unica** lectura garantizada de
  `player.CustomProperties["deaths"]`) corre en esa misma escena, esa misma
  ventana. La lectura de `ScoreBoard` ganaba la carrera casi siempre: leia el
  valor de la partida **anterior** antes de que la ida y vuelta al servidor
  terminara de aplicar el reset. `DeathsCounterDisplay`, en cambio, no sufre
  esto — no lee hasta la primera muerte propia, con los 2 s de `respawnDelay`
  de sobra para que esa misma ida y vuelta ya haya terminado.

**Arreglo:** el reset se movio de `RoomManagerNew.OnSceneLoaded()` a
`Launcher.OnJoinedRoom()` — el callback que corre apenas se entra a la sala,
**todavia en el lobby**, no en la escena de partida. Entre eso y que el
Master Client aprieta "empezar partida" (que recien ahi dispara
`PhotonNetwork.LoadLevel()`) hay de sobra para que la ida y vuelta al
servidor termine antes de que `ScoreBoard.Start()` llegue a leer nada.
`RoomManagerNew.OnSceneLoaded()` vuelve a limitarse a instanciar
`PlayerManager`, como antes de la entrada (4).

**Verificacion:** `Assembly-CSharp` compila con **0 errores**, 62 avisos
(iguales). **Sigue sin poder confirmarse sin el editor:** que el scoreboard
arranque en 0 en una partida nueva con dos clientes reales, y que ya no haya
ninguna ventana donde se vea un valor viejo aunque sea por un instante.

---

## 25/08/2026 (7)

### Regresion de prefabs: el editor de Unity abierto en paralelo piso la entrada (5)

`Photon/PhotonUnityNetworking/Resources/UI.prefab`,
`Photon/PhotonUnityNetworking/Resources/DeathsText.prefab`

Entre la entrada (5) y esta, el usuario tuvo el editor de Unity abierto
reacomodando el panel "Puntaje" a mano (nuevo tamano/posicion mas compacto,
`Muertes` y `DeathsText` pasaron de una columna estirada a una fila unica) y,
al parecer, probo arrastrar el componente `KillsCounterDisplay` directamente
sobre `DeathsText` en vez de crear un objeto propio. El resultado, verificado
con el editor ya cerrado:

- `UI.prefab`: el `m_AddedComponents` que agregaba `DeathsCounterDisplay` a la
  instancia nested de `DeathsText` habia vuelto a `[]` — la funcion de
  muertes quedo descableada de nuevo — y toda la fila "Kills" que se habia
  agregado en (5) habia desaparecido junto con eso.
- `DeathsText.prefab` (el prefab **base**, no la instancia): quedo con un
  `MonoBehaviour` extra apuntando al guid de `KillsCounterDisplay`, colgado
  del mismo GameObject que ya tiene `DeathsCounterDisplay` via override — dos
  scripts escribiendo el mismo `TMP_Text.text` en cada frame que cualquiera
  de los dos se disparara.

**Arreglo, sin volver a entrar en modo de edicion de prefab anidado (mismo
criterio que en (3)):**

- Se quito el componente extra de `DeathsText.prefab` (vuelve a sus 4
  documentos originales: GameObject, RectTransform, CanvasRenderer, TMP).
- Se restauro el `m_AddedComponents` de `DeathsCounterDisplay` sobre la
  instancia nested de `DeathsText` en `UI.prefab`, con IDs nuevos (los
  viejos ya no estaban reservados).
- La fila "Kills" se re-agrego, pero esta vez **como hermano independiente
  de "Puntaje"** (un nuevo GameObject "Kills" bajo la misma raiz), en lugar
  de como hijo de "Puntaje" — asi no vuelve a depender de la geometria que
  el usuario ya afino a mano dentro de ese panel. Se clonaron los valores
  exactos que dejo el usuario para `Muertes`/`DeathsText` (fila de 360x90,
  label a la izquierda con ancla `{0, 0.5}`, contador de 90x90 con ancla
  `{1, 0.5}`) y se coloco la fila nueva 100 unidades mas abajo del borde
  inferior de "Puntaje" — nada de lo que el usuario acomodo se toco.

**Leccion para la proxima sesion:** si el editor de Unity puede estar abierto
en paralelo, preguntar antes de tocar `.prefab` por YAML — un Auto Save o
cualquier interaccion en el Inspector puede pisar la edicion sin aviso, y el
`git diff` despues de escribir no lo distingue de una corrupcion real hasta
que se lee con cuidado.

**Verificacion:** los 135 documentos de `UI.prefab` (122 + 13 nuevos) y los 4
de `DeathsText.prefab` parsean con un lector real, sin fileIDs duplicados y
sin referencias colgantes (chequeado explicitamente sobre ambos archivos,
no solo el conteo). `Assembly-CSharp` compila con **0 errores**, 62 avisos.
**Sigue sin poder confirmarse sin el editor:** que las dos filas ("Muertes"
arriba, "Kills" abajo) se vean bien alineadas en pantalla.

---

## 25/08/2026 (6)

### Disparar sobre un cuerpo ya muerto acreditaba kills y muertes de mas

`Player/ThePlayer/LifeManager.cs`

Reportado tras probar los dos contadores en el editor: si seguias disparando
en la misma direccion despues de matar a alguien, el contador de kills
seguia subiendo por esa misma muerte, y el de muertes de la victima tambien
se inflaba de mas — los dos sintomas venian de la misma causa.

**Causa.** `SecuenciaDeMuerte()` oculta el cuerpo (`MostrarPato(false)`) y
espera `respawnDelay` (2 s) antes de destruir el controller, pero nunca
desactiva su collider. `QuitarVida()` no tenia ninguna guarda contra golpes
recibidos durante esa ventana: cada impacto extra que le pegara al cuerpo ya
"muerto" volvia a evaluar `vida <= 0f` como verdadero, y volvia a correr todo
el bloque de muerte — otro `[PunRPC] AcreditarKill` al atacante (una kill de
mas) y otro `Die()` en la victima (`Death++` y `SetCustomProperties`
otra vez, con un `PhotonNetwork.Destroy`/`CreateController()` extra de
regalo, sobre un controller que la primera `Die()` ya habia reemplazado).

**Arreglo.** Nuevo campo `muerto` en `LifeManager`. Se pone en `true` en el
mismo punto donde se decide la muerte (dentro del `if (vida <= 0f)`, antes de
acreditar la kill), y `QuitarVida()` sale temprano si ya esta puesto —
`if (!PV.IsMine || muerto) return;` — junto a la guarda existente de
`PV.IsMine`. No hace falta resetearlo en ningun lado: cada respawn es un
`LifeManager` nuevo (el controller se destruye y se recrea entero), asi que
el valor por defecto (`false`) ya es correcto para cada vida nueva.

**Verificacion:** `Assembly-CSharp` compila con **0 errores**, 62 avisos
(los mismos de antes; ninguno nuevo). **Sigue sin poder confirmarse sin el
editor:** que una rafaga sobre un cuerpo ya muerto ahora solo cuente una kill
y una muerte, con dos clientes reales.

---

## 25/08/2026 (5)

### Contador de kills propias en el HUD

`UI/newScript/KillsCounterDisplay.cs` (nuevo),
`Photon/PhotonUnityNetworking/Resources/UI.prefab`

Al revisar en el editor la entrada anterior, se aclaró qué contador faltaba de
verdad: el panel "Puntaje" solo mostraba las muertes propias (`Muertes` +
`DeathsText`, ya arreglado). Faltaba el mismo tipo de contador para las kills
propias — cuántas veces mataste en la partida — en la misma esquina superior
derecha.

**Por qué no podía copiarse tal cual `DeathsCounterDisplay`.** Ese componente
se apoya en que el controller (y por tanto el propio componente) se destruye
y se vuelve a crear en cada muerte propia, así que un `Start()` fresco ya
alcanza. Una kill no tiene ese punto de apoyo: `LifeManager.AcreditarKill()`
corre en el cliente del atacante sin destruir ni recrear su propio pato. El
nuevo `KillsCounterDisplay` es `MonoBehaviourPunCallbacks` y escucha
`OnPlayerPropertiesUpdate` (mismo patrón que ya usa `ScoreBoardItem` para el
marcador), filtrando por `PhotonNetwork.LocalPlayer` y la clave `"Kills"`.

**Prefabs.** No existía ningún `GameObject` de reserva para esto (a diferencia
de `DeathsText.prefab`, que ya estaba huérfano en el proyecto antes de esta
sesión). Se agregaron dos objetos nuevos, directamente como hijos planos de
`Puntaje` en `UI.prefab` — no como instancia de prefab anidado, porque
`Puntaje` y su label `Muertes` ya son objetos planos de `UI.prefab`, no
vienen de un sub-prefab: un label estático `"Kills"` (clon del `Text (TMP)` de
`Muertes`, mismo estilo) y `KillsText` (clon del estilo numérico de
`DeathsText`, con `KillsCounterDisplay` colgado) en la fila de abajo. Se hizo
así, sin entrar en modo de edición de prefab anidado, precisamente por los dos
incidentes de corrupción de esa técnica documentados en la entrada anterior.
`Puntaje.m_SizeDelta.y` sube de 208.8 a 300 para que entre la fila nueva.

Posiciones puestas a ojo (fila nueva 150/230 unidades por debajo del label
`Muertes`, mismo ancho de caja): son el único punto de esta entrada sin
verificar visualmente y se ajustan fácil arrastrando en el editor si no
quedan bien.

**Verificación:** los 133 documentos YAML de `UI.prefab` (124 + 9 nuevos: 4
del label, 5 del contador) parsean con un lector real, sin IDs duplicados y
sin referencias colgantes — ninguno de los 9 `fileID` nuevos falta en el
archivo. `Assembly-CSharp` compila con el Roslyn de Unity 2022.3.19f1: **0
errores**, 62 avisos (ninguno nuevo). **Sigue sin poder confirmarse sin el
editor:** que el contador suba en vivo al conseguir una kill real, y que la
posición de la fila nueva se vea bien en pantalla.

---

## 25/08/2026 (4)

### El conteo de muertes (y de kills) se arrastraba de la partida anterior

`UI/newScript/RoomManageNew.cs`

Tras confirmar en el editor que el HUD sí mostraba un número (la entrada
anterior lo dejó cableado y verificado, pero sin poder probar una muerte real
con dos clientes), quedaba una duda: ¿de dónde salía ese número si nunca había
habido una muerte en esa partida?

Se revisó el código fuente real de Photon Realtime
(`Assets/Photon/PhotonRealtime/Code/LoadBalancingClient.cs`) en vez de
especular, siguiendo el mismo método ya usado para F-53:

- `LocalPlayer` se crea **una sola vez**, en el constructor de
  `LoadBalancingClient` (línea 798). Es el mismo objeto durante toda la sesión
  de la app — sobrevive a `LeaveRoom()`/`JoinRoom()` sin recrearse.
- Al conectar al game server para entrar a una sala (tanto `CreateRoom` como
  `JoinRoom`), el propio cliente arma `allProps.Merge(this.LocalPlayer.CustomProperties)`
  y lo manda como `PlayerProperties` de esa sala (líneas 2811-2819). No es solo
  que el HUD lea un valor viejo: Photon **reenvía activamente** lo que tenga
  cacheado como el valor "oficial" de arranque del actor en la sala nueva.

Nada en el proyecto ponía `"deaths"` ni `"Kills"` a 0 al empezar partida
(`Launcher.OnJoinedRoom()` y `RoomManageNew.OnSceneLoaded()` revisados a
fondo). Con eso, jugar dos partidas seguidas sin cerrar la app hacía que la
segunda arrancara ya con el conteo de la primera — el síntoma exacto
reportado ("el contador no funciona"), aunque el campo interno `Death` de
`PlayerManager` sí se resetea bien porque vive en un objeto que se destruye al
salir de sala.

**Arreglo:** `RoomManagerNew.OnSceneLoaded()` — el punto donde ya se
instancia `PlayerManager` una vez por partida — ahora resetea
`PhotonNetwork.LocalPlayer`'s `"deaths"` y `"Kills"` a 0 justo antes de esa
instanciación. Se corrigen los dos juntos porque comparten la misma causa: el
mismo mecanismo de Photon que arrastraba `"deaths"` arrastra `"Kills"` igual,
y el marcador (`ScoreBoardItem`) ya lee ambas claves.

**Verificación:** `Assembly-CSharp` compila con el Roslyn de Unity 2022.3.19f1
(`csc.dll` vía `dotnet exec`, con las 254 referencias + 11 `ProjectReference`
resueltas contra `Library/ScriptAssemblies`, igual que en entradas previas):
**0 errores**, 62 avisos (todos preexistentes, ninguno nuevo por este cambio).
**Sigue sin poder confirmarse sin el editor:** que jugar dos partidas seguidas
efectivamente arranque en 0 con dos clientes reales.

---

## 25/08/2026 (3)

### Cableado de DeathsCounterDisplay, y corrupción de prefabs por Auto Save

`Photon/PhotonUnityNetworking/Resources/UI.prefab`

Tras crear `DeathsCounterDisplay` (entrada anterior), dos intentos de
agregarlo a mano en el editor —con **Auto Save activado** y varias instancias
de `Unity.exe` corriendo sobre el mismo proyecto— corrompieron prefabs sin
llegar a agregar el componente:

- **Intento 1:** `Pato 2.prefab` y `Pato 3.prefab` perdieron `DashBar`,
  `balasUI`, `numBalasUI`, `bazucaUI`, `revolverUI`, `thomsonUI`, `pistolaUI`,
  `sliderVida`, `sliderEscudo` y `healthBarImage` (los diez a `{fileID: 0}`).
  `UI.prefab` se reescribió casi entero (2.238 líneas), lo que sugiere que
  Unity renumeró IDs internos al entrar en su modo de edición anidado.
  Síntomas en juego: `UnassignedReferenceException` en `bazucaUI`,
  `NullReferenceException` en `ShootinController.FixedUpdate()` (repetida,
  una por frame) y el arma/pato con el contorno de selección del editor
  visible en la captura (se leyó como "la UI se rompió", pero era el
  resaltado de selección de Unity sobre el GameObject activo).
- **Intento 2:** con los tres patos ya restaurados, `Pato 1.prefab` perdió 227
  líneas de overrides del menú de Pausa (anclaje de botones, el callback
  `BackToLobby`, tamaño de fuente) — otra vez sin que `DeathsCounterDisplay`
  llegara a guardarse en ningún archivo.

Los dos intentos se revirtieron con `git checkout` sobre los archivos
afectados, verificado con `git diff --stat` después de cada uno.

**Arreglo real, sin volver a entrar en modo de edición anidado:** los tres
`Pato N.prefab` anidan el mismo `UI.prefab`, así que el componente solo hace
falta agregarlo **una vez**, ahí, no en cada pato. Se usó el mecanismo real de
Unity para agregar un componente a una instancia de prefab anidada
(`m_AddedComponents` en el bloque `PrefabInstance`), verificado antes contra
un ejemplo ya existente en el proyecto (`JumpPowerUp.prefab`) para no adivinar
el formato: un nuevo GameObject `stripped` apuntando al GameObject raíz real
de `DeathsText.prefab` (`m_CorrespondingSourceObject`), y un nuevo
`MonoBehaviour` con el guid de `DeathsCounterDisplay.cs` colgado de ese
GameObject.

**Verificación:** los 124 documentos YAML de `UI.prefab` parsean con un lector
real (0 fallos), igual que los tres `Pato N.prefab` (0 fallos, sin tocarlos —
heredan el componente nuevo automáticamente por anidamiento, sin necesitar
ninguna referencia adicional de su parte). `Assembly-CSharp` compila con **0
errores**, 79 avisos. **Sigue sin poder confirmarse sin el editor:** que el
contador suba de verdad tras una muerte real con dos clientes — recomendado
probar con Unity cerrado salvo por la instancia que se vaya a usar, y con
Auto Save desactivado mientras se navegan estos prefabs anidados, para evitar
que se repita la corrupción.

---

## 25/08/2026 (2)

### Contador de muertes propias en el HUD

`UI/newScript/DeathsCounterDisplay.cs` (nuevo), `UI/newScript/PlayerManager.cs`

El panel "Puntaje" (arriba a la derecha del HUD, dentro de `UI.prefab`) tiene
un `Text (TMP)` fijo con la palabra "Muertes" y una instancia de
`DeathsText.prefab` al lado — pero **`DeathsText.prefab` es un
`TextMeshProUGUI` crudo, sin ningún script propio**, y ningún otro script del
proyecto lo menciona por nombre (`grep` a todo `Assets/Scripts/` da cero
resultados). El campo muestra su texto por defecto ("0") para siempre; nunca
hubo código que lo conectara al conteo real de muertes.

Nuevo `DeathsCounterDisplay`, autocontenido: va en el mismo GameObject que el
`TMP_Text` y lo resuelve con `GetComponent<TMP_Text>()` en `Awake()`, sin
ninguna referencia serializada entre prefabs — evita depender de un campo
cableado a mano a tres niveles de anidamiento (`Pato N.prefab` → `UI.prefab`
→ `DeathsText.prefab`), justo el tipo de referencia frágil que ya se rompió
una vez esta sesión (Unity revirtió solo el cambio del keyword `_EMISSION` en
`pato.mat` mientras el Editor estaba abierto). En `Start()` lee
`PhotonNetwork.LocalPlayer.CustomProperties["deaths"]` una sola vez — no hace
falta escuchar `OnPlayerPropertiesUpdate`, porque el controller (y por tanto
este componente) se destruye y se vuelve a crear en cada muerte propia, así
que un valor fresco en cada `Start()` ya cubre el caso.

**Bug de orden que había que arreglar para que el valor no llegara
atrasado.** `PlayerManager.Die()` llamaba a `CreateController()` **antes** de
incrementar `Death` y escribir la `CustomProperty`. El pato nuevo (y su
`DeathsCounterDisplay`) arrancaba su `Start()` con el conteo de la muerte
*anterior*, no la que se acababa de producir. Se invirtió el orden: ahora
`Death++` y `SetCustomProperties` corren antes de `CreateController()`.

**Pendiente, a mano en el editor (no se tocó vía YAML por lo mismo de
arriba):** agregar el componente `DeathsCounterDisplay` al GameObject
`DeathsText` dentro de `UI > Puntaje` en los **tres** `Pato N.prefab` —
seleccionar `Pato N` en el Project, entrar en modo de edición de prefab,
`UI > Puntaje > DeathsText`, Add Component → `DeathsCounterDisplay`.

**Verificación:** el `.csproj` de Unity no se había regenerado todavía con el
archivo nuevo (0 coincidencias al buscarlo ahí), así que la primera pasada de
compilación lo compiló todo *menos* este script sin que se notara — se
detectó comparando el conteo de fuentes esperado contra el real. Recompilado
a mano incluyendo el archivo: **0 errores**, sin avisos nuevos.

---

## 25/08/2026

### Contador de kills

`Player/ThePlayer/LifeManager.cs`, `Emanuel_Scrips/Bala.cs`,
`UI/newScript/ScoreBoardItem.cs`, `UI/newScript/PlayerManager.cs`

El scoreboard ya tenía el campo `killsText` y lo leía correctamente de
`player.CustomProperties["Kills"]`, pero nada en el proyecto escribía esa
propiedad nunca: la única implementación era un `GetKill()`/`RPC_GetKill()`
comentado en `PlayerManager`, sin un solo llamante ni comentado ni activo. El
contador se veía en la UI pero se quedaba siempre en el valor por defecto del
prefab.

**Dónde se acredita la kill.** `QuitarVida()` ya recibía un parámetro
`PhotonMessageInfo info` pero lo ignoraba por completo. `info.Sender` es quien
mandó el RPC que aplicó el golpe — para el raycast, siempre el que dispara
(`RpcTarget.All` lo manda su propio cliente); para la bazuca, ahora también,
después del arreglo de abajo. Al golpe que deja `vida <= 0`, se manda un
`[PunRPC] AcreditarKill()` dirigido a `info.Sender`, que sí corre en el
cliente del atacante y puede escribir sus propias `CustomProperties` — un
jugador no puede escribir las de otro. Con guardado explícito
`info.Sender != PV.Owner`: sin él, volarte con tu propia bazuca te sumaría una
kill.

**Bug necesario de arreglar para que la atribución fuera confiable:**
`Bala.Explode()` no tenía ninguna guarda de red — cada cliente conectado
detecta la misma colisión por física local y la llama por su cuenta. Eso ya
estaba señalado como F-27/F-28 en la auditoría y se había dejado abierto
porque no bloqueaba nada hasta ahora: sin guarda, el daño de la bazuca se
aplicaba una vez por cliente conectado (multiplicando el daño real), y
`PhotonMessageInfo.Sender` que ve `QuitarVida` podía terminar siendo
cualquiera de esos clientes, no necesariamente el que disparó. Ahora
`Explode()` resuelve su propio `PhotonView` en `Awake()` y solo aplica daño
si `photonView.IsMine` — la explosión visual (`Instantiate` local del efecto)
y el empuje físico (`AddExplosionForce`) siguen corriendo en todos los
clientes como antes, solo el daño quedó gateado.

**Bug de mayúsculas en el listener del scoreboard.** `ScoreBoardItem.UpdateStats()`
lee `"Kills"` (con K mayúscula), pero `OnPlayerPropertiesUpdate()` comprobaba
`changedProps.ContainsKey("kills")` en minúscula — nunca iban a coincidir, así
que aunque la propiedad se hubiera actualizado, el scoreboard no se habría
refrescado en vivo para nadie que no fuera dueño de ese `ScoreBoardItem`.
Corregido a `"Kills"`.

**Código muerto retirado:** el campo `Kills` de `PlayerManager` (nunca leído:
CS0169) y el bloque comentado `GetKill()`/`RPC_GetKill()`, superado por
`LifeManager.AcreditarKill()` — este último sí tiene acceso a
`PhotonMessageInfo.Sender` en el momento del golpe, que `PlayerManager.Die()`
(llamado después, ya sin esa información) nunca pudo tener.

**Verificación:** `Assembly-CSharp` compila con **0 errores**; los avisos
bajan de 80 a 79 (se va el `CS0169` de `PlayerManager.Kills`). **Sin abrir el
editor no se puede confirmar:** que el contador suba de verdad en una partida
con dos clientes, ni que el guardado anti-suicidio (`info.Sender != PV.Owner`)
funcione como se espera con la bazuca.

---

## 23/08/2026

### Barra de vida, escudo funcional y dash

`Player/ThePlayer/LifeManager.cs`, `Player/ThePlayer/PlayerMovement.cs`,
`Emanuel_Scrips/Bala.cs`, `Resources/Pato 1|2|3.prefab`

**Barra de vida.** Convivían dos indicadores accionados por separado: el
`Slider` de `UI/Barra/vida` se escribía en cada frame y la `Image` rellenable de
`UI/Barra/backgrounVida/vida` solo dentro de `QuitarVida()`. Al regenerar vida
la Image se quedaba congelada y contradecía al slider. Además `QuitarVida()`
asignaba `fillAmount` **antes** de restar el daño, así que la barra siempre iba
un golpe por detrás. Ahora un único `RefrescarUI()` actualiza los dos, y se
llama después de aplicar el daño. `sliderVida`/`sliderEscudo` reciben su
`minValue`/`maxValue` en `Start()` en lugar de depender de lo horneado.

**Escudo.** Estaba declarado pero inerte: la lógica que le restaba daño estaba
comentada, `ObtenerEscudo()` estaba vacío y los prefabs traían `escudo: 0`. Se
implementó absorción real —el escudo consume el daño primero y solo el sobrante
llega a la vida— y regeneración con prioridad al escudo. En los prefabs
`tiempoParaRegen` y `cantidadDeRegeneracion` valían **0**, de modo que la
regeneración era instantánea y de cero puntos; pasan a 5 s y 20 puntos/s. La
tasa era `cantidadDeRegeneracion / 100` por `FixedUpdate` (dependiente del paso
físico); ahora es por segundo con `Time.deltaTime`.

**Espera antes de regenerar.** La pareja `danorecibido`/`contador` con corrutina
de 2 s se relanzaba en cada impacto: al encadenar dos golpes, la corrutina del
primero bajaba la bandera y la regeneración arrancaba antes de tiempo. Se
sustituye por un único acumulador `tiempoSinDano`.

**Daño de la bazuca (no se verificó nunca en red).** `Bala.Explode()` llamaba a
`QuitarVida()` directamente sobre el `LifeManager` del pato alcanzado. Eso corre
en el cliente que disparó, donde ese pato no es suyo, así que la guarda
`if (!PV.IsMine) return` cortaba y **la explosión no quitaba vida a nadie**.
Ahora usa `TakeDamage()`, que enruta el golpe por RPC hasta el propietario.
`TakeDamage()` no tenía ni un solo llamante hasta ahora.

**Dash.** Ya existía asociado a `F`, pero:

- `sliderDash` está tipado como `Slider` y en los tres prefabs apuntaba a un
  **`TextMeshProUGUI`** (el contador de balas `UI/Balas/Arma/NumBalas`). Unity
  resuelve a `null` una referencia de tipo incompatible, así que `Start()`
  lanzaba `NullReferenceException` en su primera línea —dejando
  `playerPhotonSoundManager` sin asignar, que es lo que usan los power-ups de
  velocidad y salto— y `Update()` repetía la excepción **en cada frame**. El
  slider se obtiene ahora desde `DashBar`, que sí apunta a `UI/Barra/dash`.
- La corrutina esperaba `0.01 s`, menos de un frame a 60 fps, y ponía
  `canDash = false` *después* del `yield`: el impulso duraba un frame y el
  bloqueo llegaba tarde. Ahora hay `duracionDash` (0,15 s) y el bloqueo es
  inmediato.
- La barra de enfriamiento recibía `Time.deltaTime` y luego `Time.time`, que
  crece sin límite; quedaba clavada al máximo. Ahora va de 0 a 1 sobre `CDDash`.
- `dashModifier` baja de **20 a 4**: con una duración real de 0,15 s, 20× habría
  desplazado al pato unas 30 unidades de golpe.

No necesita RPC: el `PhotonView` del pato observa un `PhotonTransformView`, así
que el desplazamiento ya viaja al resto de clientes.

**Código muerto retirado:** `isLocalPlayer`, `healthBar`, `shieldBar`,
`originalHealthBarSize`, `originalShieldBarSize`, `ObtenerVida()`,
`ObtenerEscudo()` y `Danorecibido()` en `LifeManager`; `dashTime`,
`timeDashSize`, `PV` y `playerManager` en `PlayerMovement`. Ninguno tenía
lectores fuera de su propio archivo.

**Verificación:** `Assembly-CSharp` compila con **0 errores**; los avisos bajan
de 81 a 79 (los dos `CS0169` de `PlayerMovement`). Los tres prefabs se
reparsearon con un lector YAML real (214/168/168 documentos, 0 fallos). Falta
por probar en el editor con dos clientes: el daño de la bazuca y la muerte con
escudo activo.

**Qué acciona cada barra.** El contenedor `UI/Barra` se reactivó a mano en los
tres patos. Dentro de él, el GameObject `UI/Barra/vida` (un `Slider`) sigue
desactivado, así que la barra de vida que se ve es la `Image` de
`UI/Barra/backgrounVida/vida` — tipo *Filled*, método *Horizontal*, con sprite —
y se acciona por `fillAmount`. Escudo (`UI/Barra/Escudo`) y dash
(`UI/Barra/dash`) sí son sliders activos, con su hijo `Fill` presente y con
sprite, y se accionan por `value`. `RefrescarUI()` cubre los tres casos.

**Efecto en el equilibrio:** con `escudoMax = 100` sobre `vidaMax = 100` el pato
pasa a aguantar 200 puntos de daño efectivo. El revólver (25) necesita ahora 8
impactos para matar en vez de 4. Si resulta demasiado, el ajuste es bajar
`escudoMax` en los tres prefabs.

---

## 23/08/2026 (2)

### Flash de golpe y VFX de muerte

`Player/ThePlayer/LifeManager.cs`, `UI/newScript/PlayerManager.cs`,
`Resources/Pato 1|2|3.prefab`

**Flash de golpe.** `QuitarVida()` ahora manda un `[PunRPC]` (`FlashDeGolpe`,
`RpcTarget.All`) que tiñe el `SkinnedMeshRenderer` del pato en rojo si el golpe
llegó a la vida, o en cian si lo absorbió por completo el escudo. Se ve en
todos los clientes, incluido el que disparó — antes no había ningún feedback
visual sobre el propio pato al recibir daño.

Se usa `MaterialPropertyBlock` en vez de `renderer.material`: lo segundo
instancia una copia del material en cada impacto (las dos mallas de Pato 1
comparten `pato.mat`, así que se habrían desincronizado y habría fugado
memoria). El material del pato no tiene el keyword `_EMISSION` activo —
`_EmissionColor` no habría hecho nada — así que se tiñe `_BaseColor`
directamente.

Pato 2 y Pato 3 sacan su malla de un FBX anidado (`Rig_Duck_2`) en vez de
traerla inline como Pato 1, así que los renderers se resuelven con
`GetComponentsInChildren<Renderer>()` en `Awake()` en lugar de una referencia
serializada — cubre los tres skins sin tener que cablear nada a mano.

**SFX de golpe y muerte en los tres skins.** `LifeManager.playerPhotonSoundManager`
solo estaba cableado en Pato 1; en Pato 2 y 3 el campo del inspector venía
vacío, así que `PlayHurtSFX()` estaba comentado. Ahora se resuelve con
`GetComponent<PlayerPhotonSoundManager>()` en `Awake()` si el campo llega
vacío, y se llama tanto al recibir daño como al morir.

`PlayerManager.playerPhotonSoundManager` era un campo muerto: se asignaba con
`GetComponent<PlayerPhotonSoundManager>()` en `Start()`, pero ese componente
no existe en `PlayerManager.prefab` — siempre valía `null`. Por eso la llamada
a `PlayDieSFX()` estaba comentada; si se hubiera descomentado tal cual habría
lanzado `NullReferenceException`. Se retiró el campo y la asignación; el SFX de
muerte ahora sale desde `LifeManager`, que sí tiene un
`PlayerPhotonSoundManager` real en el mismo GameObject.

**VFX de muerte.** `PlayerManager.Die()` destruía el controller y creaba uno
nuevo en la misma línea — respawn instantáneo, sin ninguna animación ni pausa.
Como la cámara vive dentro del prefab del pato (`PlayerSetUp._camara`,
`_camaraCinemachine`), destruirlo y esperar antes de crear el siguiente habría
dejado un hueco sin ninguna cámara activa — la misma pantalla en negro que ya
documenta F-54 en este changelog.

En vez de eso, `LifeManager` añade una `SecuenciaDeMuerte()` que:

1. Desactiva `PlayerMovement` y `ShootinController` (el pato no se mueve ni
   dispara mientras está "muerto").
2. Instancia `Explosión pato.prefab` — un asset que ya existía en
   `Resources/`, con partículas de plumas, `PhotonView` y
   `PhotonTransformView`, pero al que no lo referenciaba ni un script ni una
   escena ni otro prefab. Ya trae `VFX_Destroyer`, así que se autodestruye
   solo.
3. Manda `[PunRPC] MostrarPato(false)` a todos los clientes, que apaga todos
   los `Renderer` del pato (cuerpo y arma activa) en sincronía con la
   explosión.
4. Espera `respawnDelay` (2 s por defecto).
5. Solo entonces llama a `PlayerManager.Die()`, que sigue haciendo lo mismo de
   siempre: `PhotonNetwork.Destroy` + `CreateController()`.

El controller viejo se mantiene vivo (solo oculto) durante la espera
precisamente para que su cámara siga renderizando. `PlayerManager.Die()` no
cambió su lógica interna — solo pasó a ejecutarse más tarde, después del hueco
de muerte.

**Prefabs:** se añadió `deathVfx: {fileID: 413361622400378864, guid:
f76c2c5066af9ef429a156d3fc6fdad4, type: 3}` (referencia a `Explosión pato.prefab`)
al bloque de `LifeManager` en los tres `Pato N.prefab`, a mano en el YAML — el
resto de campos nuevos (`respawnDelay`, `colorFlashVida`, `colorFlashEscudo`,
`duracionFlash`) toman el valor por defecto del script porque nunca existieron
en el prefab.

**Verificación:** `Assembly-CSharp` compila con **0 errores**; el único aviso
nuevo es el `CS0649` esperado de `deathVfx` (todo `[SerializeField]` sin
asignación visible para el compilador se marca así — no indica que el cableado
del prefab haya fallado; eso se verificó aparte con un lector YAML real, 211/
165/168 documentos, 0 fallos). **Sin abrir el editor no se puede confirmar**:
que el flash se vea con el color correcto, que la explosión se vea en todos
los clientes, ni que la cámara efectivamente se quede viendo la escena durante
el hueco de muerte — las tres necesitan una prueba en el editor con dos
clientes.

---

## 24/08/2026

### Flash de golpe más notorio y orden de la secuencia de muerte

`Player/ThePlayer/LifeManager.cs`, `Scenes/DANI/Animaciones/Propias/Pato 1/pato.mat`,
`Scenes/DANI/Animaciones/Propias/Pato 2/pato.mat`, `Scenes/DANI/PatosModelosTesxturas/Textura pato3.mat`

Tras la primera prueba en el editor con varios clientes, se reportaron tres
problemas sobre el flash de golpe y el VFX de muerte del 23/08/2026 (2):

1. El VFX de muerte se veía bien para el jugador que muere, pero el resto veía
   primero la explosión, luego el pato todavía en pie, y solo después
   desaparecía.
2. El flash de golpe solo lo veía el jugador que recibía el daño.
3. Ese flash se veía muy tenue incluso para quien sí lo veía.

**Diagnóstico del punto 1 y 2 (RPCs entre clientes).** Se revisó el código
fuente real de Photon PUN2 (`Assets/Photon/PhotonUnityNetworking/Code/PhotonNetworkPart.cs`,
`ExecuteRpc` y `RPC`) para descartar explicaciones especulativas:

- Los métodos `[PunRPC]` privados sí se ejecutan: `ExecuteRpc` invoca por
  reflexión (`mInfo.Invoke`) sin filtrar por visibilidad, y tampoco filtra por
  `MonoBehaviour.enabled` — `RefreshRpcMonoBehaviourCache()` usa
  `GetComponents<MonoBehaviour>()`, que incluye componentes desactivados. La
  hipótesis de que `PlayerSetUp` desactiva `LifeManager` en los patos remotos
  y por eso no llegan los RPC queda descartada.
- `StartCoroutine` funciona igual con el componente desactivado, mientras el
  GameObject siga activo — tampoco explica que `FlashDeGolpe` no se vea.
- El nombre de ambos métodos ya estaba registrado en
  `PhotonServerSettings.asset` → `RpcList` (se comprobó directamente en el
  archivo), así que no hay desajuste de índice cliente-a-cliente por ese lado
  — y aunque no lo estuviera, el código cae a enviar el nombre completo como
  string, que cualquier receptor interpreta igual.

No se encontró ningún defecto en el mecanismo de RPC en sí. La explicación más
probable que **no** se pudo descartar sin acceso a la consola de un cliente no
propietario en el momento del golpe: **algún cliente de prueba corriendo un
build viejo**, compilado antes de que `FlashDeGolpe`/`MostrarPato` existieran
— ese build no tiene el método en su ensamblado y el mensaje se ignora en
silencio para ese cliente en particular. Si las pruebas mezclan un .exe
exportado antes de esta sesión con el editor (que sí tiene el código
actualizado), eso solo explicaría el problema para el build viejo, no para el
resto. **Pendiente de confirmar**: si el problema persiste tras recompilar
todos los clientes desde el código actual, hace falta revisar la consola de un
cliente que no sea el dueño del pato en el instante del golpe/muerte.

**Arreglo aplicado de todos modos: orden de los dos mensajes de red en la
muerte.** Aunque no se confirmó un defecto de RPC, `SecuenciaDeMuerte()` sí
mandaba el `Instantiate` del VFX antes que el RPC de ocultar — dos mensajes de
red independientes que no tienen por qué llegar juntos. Se invirtió el orden
(ocultar primero, VFX después) para que en el peor caso el pato desaparezca a
la vez o antes que la explosión, no después.

**Arreglo del punto 3 (muy tenue).** El flash solo tocaba `_BaseColor`
(albedo): ese valor lo multiplica la luz de la escena antes de llegar a
pantalla, así que se atenúa según el ángulo de cámara, las sombras y la
distancia — exactamente el tipo de cosa que se ve bien de cerca y de frente
pero débil desde otro ángulo. `pato.mat` traía además el keyword `_EMISSION`
desactivado (confirmado en los tres materiales de piel: `Pato 1/pato.mat`,
`Pato 2/pato.mat` y `Textura pato3.mat`, los tres con el mismo shader URP/Lit),
así que escribir `_EmissionColor` por script no habría hecho nada.

Se activó `_EMISSION` en los tres materiales (`m_ValidKeywords: [_EMISSION]`),
dejando `_EmissionColor` en negro por defecto — sin cambio visible en reposo,
mismo truco que ya usa `PlayerContrast_MTL.mat` en el proyecto. El flash ahora
también escribe `_EmissionColor` a `color * intensidadFlash` (HDR, por defecto
6x) vía `MaterialPropertyBlock`: la emisión se suma al resultado ya iluminado
en vez de multiplicarse por la luz ambiente, así que se ve igual de fuerte sin
importar el ángulo de cámara ni la iluminación de la escena.

No se sabía con certeza qué material usan de piel Pato 2 y Pato 3 en tiempo de
ejecución: ambos comparten el mismo FBX anidado (`Rig_Duck_2.fbx`) sin ningún
override de material en el prefab, así que el material lo resuelve el
importador de Unity por nombre. Se activó el keyword en los tres candidatos
(`Pato 1/pato.mat`, `Pato 2/pato.mat`, `Textura pato3.mat`) para cubrir
cualquiera que termine siendo el real, en vez de adivinar cuál.

**Verificación:** `Assembly-CSharp` compila con **0 errores**, 80 avisos
(iguales a la entrada anterior). Los tres `.mat` se revisaron a mano
(`m_ValidKeywords` bien formado, mismo patrón que un material ya existente en
el proyecto). **Sigue sin poder verificarse sin el editor:** si el flash y el
ocultamiento ahora sí llegan a todos los clientes cuando se prueba con
builds actualizados, y si la intensidad de 6x resulta demasiado o poco.

---

## 21/08/2026

### F-54 · No se podía jugar una segunda partida sin reiniciar la app

`Scenes/Rolix/Online 2.unity`

Al terminar una partida y volver al menú, la consola mostraba
`PhotonView ID duplicate found: 999` seguido de una `InvalidOperationException:
Duplicate key 999` **sin capturar**, y la pantalla de sala quedaba rota
(`No cameras rendering`, restos de otra pantalla superpuestos) sin responder.

Causa, confirmada cruzando la consola con el código fuente de Photon:

- El GameObject «Room Manager» de `Online 2.unity` (dueño de `RoomManagerNew`)
  tenía **también** un componente `PhotonView`, con un `sceneViewId` fijo
  (`999`) horneado en el archivo de escena.
- `RoomManagerNew` se marca `DontDestroyOnLoad`. Al volver del menú tras la
  partida, `ReturnToMenu()` recarga `Online 2.unity` desde cero: nace un
  segundo «Room Manager», con el **mismo** `sceneViewId`.
- La guarda de singleton en código (`if (instance) Destroy(gameObject)`) no
  llega a tiempo: el registro del `PhotonView` ocurre en el setter de
  `ViewID` (`PhotonView.cs:319`), independiente del orden de `Awake()` entre
  componentes hermanos, y `Destroy()` es diferido al final del frame.
- El propio manejo de duplicados de Photon está roto:
  `RegisterPhotonView` (`PhotonNetworkPart.cs:987-1022`) registra el aviso y
  llama a `RemoveInstantiatedGO(...)`, pero esa baja no es síncrona — el
  `photonViewList.Add(...)` que sigue revienta sin que nada lo capture.

Arreglo: **`RoomManagerNew.cs` nunca usa su propio `PhotonView`** — no llama a
`photonView.RPC(...)` en ningún sitio; solo necesita ser
`MonoBehaviourPunCallbacks` para recibir los callbacks globales de Photon, lo
que no requiere tener un `PhotonView` propio. Se quitó el componente
`PhotonView` del GameObject «Room Manager» directamente en el YAML de la
escena — sin tocar ningún `.cs`. Verificado con un lector YAML real: la lista
de componentes del GameObject pasa de 3 a 2 (`Transform` + `RoomManagerNew`),
sin referencias huérfanas al `fileID` eliminado en el resto del archivo.

**Por qué no había forma de que el código lo evitara solo:** el choque ocurre
en la capa de registro de Photon, que corre independientemente del orden de
inicialización de los componentes de este proyecto. Quitar el `PhotonView` no
es un parche — es quitar la causa: sin un `ViewID` que fijar, no hay id que
pueda chocar.

### F-53 · Doble clic en «empezar partida» congelaba el cliente entero

`UI/newScript/Launcher.cs`, `docs/lobby-transiciones.feature`

Hallazgo **nuevo**, reportado desde el editor: pulsar varias veces el botón de
empezar partida y luego «Volver» dejaba la pantalla en «Cargando...» para
siempre, con 11 entradas `Online 3 (is loading)` colgando de la jerarquía.

Cadena completa, reconstruida sobre el código de PUN:

1. `StartGame()` no tenía guarda: la escena vieja sigue viva mientras
   `Online 3` carga en segundo plano, así que el botón seguía siendo clicable.
2. Cada clic llamaba otra vez a `PhotonNetwork.LoadLevel()`. Photon detecta la
   carga en curso, la marca `allowSceneActivation = false` y **pierde la
   referencia** (`PhotonNetworkPart.cs:2166-2174`): queda huérfana, congelada,
   sin cancelarse de verdad. Una por clic.
3. Con varias cargas `Single` solapadas y pausadas a medias, ninguna llega a
   completarse.
4. `LoadLevel()` pone `IsMessageQueueRunning = false` (`PhotonNetwork.cs:3068`)
   y solo lo restaura `NewSceneLoaded()` cuando Unity dispara `sceneLoaded`
   (`PhotonNetworkPart.cs:1468-1473`). Como ninguna escena termina, esa bandera
   se queda en `false` **para siempre**.
5. Esa bandera gobierna el bucle de envío (`PhotonHandler.cs:172`, `:185`) y el
   de recepción (`:226`). Con ella en `false` el cliente deja de enviar y
   recibir todo.
6. «Volver» → `Launcher.LeaveRoom()`: abre el menú «loading» y llama a
   `PhotonNetwork.LeaveRoom()`, que nunca llega a salir del cliente. `OnLeftRoom`
   no dispara y la pantalla se queda en «Cargando...».

Arreglo, sin tocar código de Photon (es paquete de terceros y se revertiría en
la siguiente actualización):

- Nueva bandera `isTransitioning` con `TryBeginTransition()` / `EndTransition()`.
  Se aplica a las **cuatro** operaciones de red disparadas por botón:
  `CreateRoom`, `JoinRoom`, `StartGame` y `LeaveRoom`.
- `StartGame()` además oculta el botón de inmediato, como defensa en profundidad.
- Todos los callbacks cierran la transición: `OnJoinedRoom`, `OnLeftRoom`,
  `OnJoinedLobby`, `OnCreateRoomFailed`, `OnJoinRoomFailed` y `OnDisconnected`.
  Si un fallo no la cerrara, los botones quedarían mudos el resto de la sesión.
- Se comprueba el `bool` que devuelven `CreateRoom`, `JoinRoom` y `LeaveRoom`:
  las tres pueden fallar en el sitio sin lanzar ningún callback, y ese `false`
  ignorado dejaba la bandera atascada.
- La escena `"Online 3"` pasa a constante `GameSceneName`.

**Un fallo de mi primer diseño, para que quede registrado:** `TryBeginTransition()`
llamaba a `ShowError()`, que reseteaba la bandera. El segundo clic (rechazado)
desbloqueaba la transición en curso y el tercero volvía a colar un `LoadLevel`
— el bug otra vez. Por eso `ShowError()` ya no toca la bandera y el cierre es
explícito en cada callback. Está recogido como hipótesis de mutación nº 4.

**Entregables de la skill `unity-clean-code`:** el Gherkin y el plan de QA y
mutación están en `docs/lobby-transiciones.feature`. Los tests NUnit **no** se
escribieron: `Launcher` llama directamente a la clase estática `PhotonNetwork`,
así que no es testeable en EditMode sin extraer antes una interfaz inyectable.
El `.feature` documenta esa deuda y los pasos concretos para saldarla.

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

**El orden de arreglo acordado (bloques 1 a 8) está completo:** 22 hallazgos
corregidos y 2 parciales (F-16 y F-50). Quedan 29 del informe original sin
prioridad asignada, más los que vayan apareciendo al probar en el editor
(F-53 salió así): sobre todo los de rendimiento y código muerto
del bloque E del informe, más los de convenciones y repositorio.

El siguiente natural, por quedar a un paso de un cambio ya hecho:

| Hallazgo | Qué | Dónde |
|---|---|---|
| F-33 | Usar `deathsTarget` en vez del literal `5`, y `>=` en vez de `==` | `PlayerManager.WinningConditions()` |

Sigue abierto también **F-27** (`Bala.Explode()` sin guarda de reentrada) y
**F-28** (daño de explosión aplicado en local), que se dejaron intactos al
tocar `Bala.cs` para F-25 por ser hallazgos propios.
