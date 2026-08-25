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
