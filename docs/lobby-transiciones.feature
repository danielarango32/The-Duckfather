# language: es
# Especificacion del hallazgo F-53: transiciones de red del lobby.
#
# Escrita como especificacion ejecutable-en-intencion. El proyecto todavia no
# tiene infraestructura de tests (ni carpeta Tests/ ni .asmdef de test), y estos
# escenarios dependen de PhotonNetwork, que es estatico. Para automatizarlos
# haria falta primero envolver esa dependencia tras una interfaz inyectable
# (ver "Deuda para automatizar" al final).

Caracteristica: Una sola operacion de red del lobby a la vez
  Para no dejar el cliente colgado
  Como jugador impaciente que pulsa los botones varias veces
  Quiero que el segundo clic se ignore en lugar de encadenar otra operacion

  Antecedentes:
    Dado que estoy conectado al Master Server
    Y estoy dentro de una sala como MasterClient

  # --- el bug reportado --------------------------------------------------
  Escenario: Pulsar "empezar partida" varias veces no encadena cargas de escena
    Cuando pulso "empezar partida" 11 veces seguidas
    Entonces solo se llama a PhotonNetwork.LoadLevel una vez
    Y el boton queda oculto tras el primer clic
    Y PhotonNetwork.IsMessageQueueRunning vuelve a true al cargar la escena
    Y la jerarquia no contiene escenas "Online 3 (is loading)" huerfanas

  Escenario: "Volver" sigue respondiendo despues de intentar empezar la partida
    Cuando pulso "empezar partida" 11 veces seguidas
    Y pulso "Volver"
    Entonces la peticion de salir de la sala llega a enviarse
    Y acabo en el menu principal
    # Antes: la cola de mensajes estaba parada, LeaveRoom nunca salia del
    # cliente y la pantalla se quedaba en "Cargando..." indefinidamente.

  # --- las otras tres operaciones ----------------------------------------
  Esquema del escenario: Un segundo clic se ignora mientras hay algo en curso
    Cuando pulso "<boton>" dos veces seguidas
    Entonces la operacion de Photon se dispara una sola vez
    Y se registra un aviso de que habia una operacion en curso

    Ejemplos:
      | boton            |
      | crear sala       |
      | entrar a la sala |
      | Volver           |
      | empezar partida  |

  # --- la bandera nunca se queda atascada --------------------------------
  Escenario: Un fallo libera la guarda
    Dado que existe una sala llamada "duckfather"
    Cuando pulso "crear sala" con el nombre "duckfather"
    Y Photon responde con OnCreateRoomFailed
    Entonces veo el motivo del fallo
    Y puedo volver a pulsar "crear sala" sin reiniciar el juego

  Escenario: Una desconexion libera la guarda
    Cuando pulso "entrar a la sala"
    Y se pierde la conexion antes de la respuesta
    Entonces la guarda queda liberada
    Y los botones vuelven a responder al reconectar

  Escenario: Salir de una sala en la que no estoy no bloquea el lobby
    Dado que ya no estoy en ninguna sala
    Cuando pulso "Volver"
    Entonces PhotonNetwork.LeaveRoom devuelve false
    Y acabo en el menu principal
    Y la guarda queda liberada
    # OnLeftRoom no llega en este caso: si no se cerrara aqui, todos los
    # botones quedarian mudos el resto de la sesion.

# ---------------------------------------------------------------------------
# QA: valores limite y ramas
# ---------------------------------------------------------------------------
#  - 0 clics  -> ninguna operacion, guarda en false.
#  - 1 clic   -> exactamente una operacion, guarda en true hasta el callback.
#  - 2 clics  -> una operacion; el segundo entra por la rama de rechazo.
#  - N clics  -> identico a 2 (la rama de rechazo no muta estado).
#  - Las 4 operaciones x {exito, fallo, desconexion} = 12 caminos de cierre.
#
# ---------------------------------------------------------------------------
# Hipotesis de mutacion (que test las mataria)
# ---------------------------------------------------------------------------
#  1. Invertir la guarda: `if (!isTransitioning)` en TryBeginTransition
#     -> muere con "pulsar 11 veces": se dispararian 11 LoadLevel.
#  2. Quitar `isTransitioning = true` antes del return true
#     -> muere con "pulsar dos veces": la segunda pasaria.
#  3. Que TryBeginTransition devuelva siempre true
#     -> muere con cualquier escenario de doble clic.
#  4. Reintroducir EndTransition() dentro de la rama de rechazo
#     -> muere con "pulsar 11 veces": el tercer clic volveria a colar una
#        operacion. Este es el fallo concreto que tuvo la primera version
#        del arreglo, y por eso ShowError ya no toca la bandera.
#  5. Quitar EndTransition() de OnCreateRoomFailed / OnJoinRoomFailed
#     -> muere con "Un fallo libera la guarda".
#  6. Quitar EndTransition() de OnDisconnected
#     -> muere con "Una desconexion libera la guarda".
#  7. Ignorar el bool de PhotonNetwork.LeaveRoom()
#     -> muere con "Salir de una sala en la que no estoy".
#  8. Quitar startGameButton.SetActive(false) de StartGame
#     -> NO muere con los escenarios logicos (la guarda sola ya basta); solo
#        lo detecta la comprobacion de UI "el boton queda oculto". Es defensa
#        en profundidad, no la correccion principal.
#
# ---------------------------------------------------------------------------
# Deuda para automatizar esto
# ---------------------------------------------------------------------------
# Launcher llama directamente a la clase estatica PhotonNetwork, asi que no es
# testeable en EditMode. Para automatizar estos escenarios:
#   1. Extraer una interfaz (p. ej. INetworkService) con CreateRoom, JoinRoom,
#      LeaveRoom, LoadLevel y las propiedades de estado que se consultan.
#   2. Inyectarla en Launcher; en produccion, un adaptador sobre PhotonNetwork.
#   3. Los escenarios de arriba pasan a EditMode con un doble de prueba que
#      cuenta llamadas. No hace falta PlayMode: no hay fisicas ni corutinas.
