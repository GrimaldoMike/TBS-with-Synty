using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerInputMenu : MonoBehaviour
{
    // <>
    public static PlayerInputMenu instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject inputMenu, moveMenu, meleeMenu, fireMenu, moveReturnMenu, spawnerBG;

    public TMP_Text turnPointsText, errorText;
    private Vector3 originalPositionErrorText;
    public float errorDisplayTime = 2f;
    private float errorCounter;

    public TMP_Text hitChanceText;

    public TMP_Text resultText;
    public GameObject startBattleButton, endBattleButton;

    public GameObject pauseScreen;
    public AudioSource music;

    public TMP_Text battleStateLabel, moveMapInfoLabel;
    public GameObject battleStatePlayerLabel, battleStateEnemyLabel;
    public GameObject unitsMenuButton, showMapButton;

    //Cutscene variables
    public GameObject isCutsceneOver;
    private void Start()
    {
        originalPositionErrorText = errorText.gameObject.transform.position;
    }

    private void Update()
    {
        if (GameManager.instance.isPaused == false)
        {
            ShowHideErrorMessage();
        }
        KeyInputController();
    }

    private void KeyInputController()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
        }
    }

    public void HideMenus()
    {
        inputMenu.SetActive(false);
        moveMenu.SetActive(false);
        moveReturnMenu.SetActive(false);
        meleeMenu.SetActive(false);
        fireMenu.SetActive(false);
        turnPointsText.gameObject.SetActive(false);
        errorText.gameObject.SetActive(false);
        battleStateLabel.gameObject.SetActive(false);
        battleStatePlayerLabel.SetActive(false);
        battleStateEnemyLabel.SetActive(false);
        startBattleButton.SetActive(false);
        endBattleButton.SetActive(false);
        unitsMenuButton.SetActive(false);
        showMapButton.SetActive(false);
        spawnerBG.SetActive(false);
        moveMapInfoLabel.gameObject.SetActive(false);
        ShowHideTargetDisplay(false); //Cuando se regresa al menu principal, se oculta el indicador target display.
        
        if(GameManager.instance.activePlayer is not null)
        {
            if (GameManager.instance.activePlayer.isShooting == false) //Solamente no regreses la camara al personaje activo cuando no estoy disparando.
                CameraController.instance.SetMoveTarget(GameManager.instance.activePlayer.transform.position);
        }

    }

    public void ShowInputMenu()
    {
        inputMenu.SetActive(true);
        ShowHideTargetDisplay(false); //Se regresa al menu principal, se oculta el indicador target display.
        turnPointsText.gameObject.SetActive(true);
    }

    public void ShowMoveMenu()
    {
        HideMenus();
        moveMenu.SetActive(true);
        turnPointsText.gameObject.SetActive(true);
        ShowMove(); //Se activa Move por default cuando se presiona "Move", entonces se muestra el rango de Walk.

        SFXManager.instance.UISelect.Play();
    }

    //Menu para regresar a la posici�n anterior despu�s de moverte pero antes de aceptar y gastar turnPoints.
    public void ShowMoveReturnMenu()
    {
        HideMenus();
        moveReturnMenu.SetActive(true);
        turnPointsText.gameObject.SetActive(true);
        UpdateTurnPointsText(-1);
    }

    //Despu�s de mover o correr, debes aceptar el movimiento de a donde llegaste.
    public void AcceptMovement()
    {
        //GameManager.instance.FinishedMovement();
        HideMenus();
        StartCoroutine(WaitToEndActionCo(0.5f));
        SFXManager.instance.UISelect.Play();
    }

    //Se cancela el movimiento anterior hecho. No se pierden los puntos y puedes decidir ir a otro lugar
    public void ReturnMovement()
    {
        GameManager.instance.ReturnToPoint();
        HideMenus();
        UpdateTurnPointsText(GameManager.instance.turnPointsRemaining); // Sirve para quitar el mensaje de "Move here?" y dejar los turn points que tenia
        turnPointsText.gameObject.SetActive(true);
        moveMenu.SetActive(true);
        ShowMove();
        SFXManager.instance.UICancel.Play();
    }

    //Funcionalidad para el boton "Cancel". 
    public void HideMoveMenu()
    {
        HideMenus();
        MoveGrid.instance.HideMovePoints();
        ShowInputMenu();
        SFXManager.instance.UICancel.Play();
    }
    public void ShowMove()
    {
        //Muestra el grid de movimiento solo cuando el bot�n fue presionado y existan acciones por turno restantes.
        if (GameManager.instance.turnPointsRemaining >= 1)
        {
            //MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.moveRange, GameManager.instance.activePlayer.transform.position);
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.characterData.CurrentStats.CurrentWalkRange, GameManager.instance.activePlayer.transform.position);
            GameManager.instance.currentActionCost = 1;
        }
        SFXManager.instance.UISelect.Play();
    }

    public void ShowRun()
    {
        //Mismo que ShowMove, pero se usa la variable "runRange".
        if (GameManager.instance.turnPointsRemaining >= 2)
        {
            //MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.runRange, GameManager.instance.activePlayer.transform.position);
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.characterData.CurrentStats.CurrentRunRange, GameManager.instance.activePlayer.transform.position);
            GameManager.instance.currentActionCost = 2;
        }
        SFXManager.instance.UISelect.Play();
    }

    //Actualiza el texto de "Turn Points Remaining" del UI.
    public void UpdateTurnPointsText(int turnPoints)
    {
        turnPointsText.text = "Turn Points Remaining: " + turnPoints;
        if(turnPoints == -1)
        {
            turnPointsText.text = "Move here?: ";
        }
    }

    //Boton de "Skip Turn"
    public void SkipTurn()
    {
        GameManager.instance.EndTurn();
        SFXManager.instance.UISelect.Play();
    }
    public void ShowMeleeMenu()
    {
        HideMenus();
        meleeMenu.SetActive(true);
        SFXManager.instance.UISelect.Play();
    }
    public void HideMeleeMenu()
    {
        HideMenus();
        ShowInputMenu();
        SFXManager.instance.UICancel.Play();
    }

    //Revisa si habilita el bot�n de melee para ser cliqueado: si hay objetivos de melee, lo puede presionar, sino se desactiva.
    public void CheckMelee()
    {
        GameManager.instance.activePlayer.GetMeleeTargets();

        if(GameManager.instance.activePlayer.meleeTargets.Count > 0) //Si encontre objetivos en rango de melee.
        {
            ShowMeleeMenu();
            //Se activa el indicador de Target encima del modelo activo.
            //GameManager.instance.targetDisplay.SetActive(true);
            ShowHideTargetDisplay(true);
            GameManager.instance.targetDisplay.transform.position = GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform.position;
            GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform); //Mandamos rotar al jugador al decidir a cu�l enemigo mirar.

        }
        else
        {
            ShowErrorText("No enemies whitin melee range");
            SFXManager.instance.UICancel.Play();
        }
    }

    //Funcion para las acciones de ataques melee
    public void MeleeHit()
    {
        GameManager.instance.activePlayer.DoMelee();
        GameManager.instance.currentActionCost = 1; // Se setea el costo de la acci�n de melee.

        HideMenus(); //Ocultamos menus para visualizar la animacion de melee.
        StartCoroutine(WaitToEndActionCo(1f)); // Se controla el final de turno por puntos gastados con corrutina de esperar.
        SFXManager.instance.UISelect.Play();
    }

    //Se guarda un contador de objetivos alrededor del jugador. Se suma 1 cuando se manda llamar, se resetea si se sobrepasa del l�mite.
    public void NextMeleeTarget()
    {
        GameManager.instance.activePlayer.currentMeleeTarget++;
        if(GameManager.instance.activePlayer.currentMeleeTarget >= GameManager.instance.activePlayer.meleeTargets.Count)
        {
            GameManager.instance.activePlayer.currentMeleeTarget = 0;
        }
        GameManager.instance.targetDisplay.transform.position = GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform.position;
        GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform); //Mandamos rotar al jugador al decidir a cu�l enemigo mirar.
        SFXManager.instance.UISelect.Play();

    }
    //Servir� para permitir a las animaciones terminar de ejecutarse, de esperar algun movimiento cinematico, de camara, de loading de menu, etc.
    public IEnumerator WaitToEndActionCo(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);

        GameManager.instance.SpendTurnPoints();
        CameraController.instance.SetMoveTarget(GameManager.instance.activePlayer.transform.position);

    }
    public IEnumerator WaitToStartPhase(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        battleStateLabel.gameObject.SetActive(false);
        battleStatePlayerLabel.SetActive(false);
        battleStateEnemyLabel.SetActive(false);

        GameManager.instance.EndTurn();

        CameraController.instance.SetMoveTarget(GameManager.instance.activePlayer.transform.position);
    }
    public IEnumerator WaitToEndBattlePhases(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        battleStateLabel.gameObject.SetActive(false);
        battleStatePlayerLabel.SetActive(false);
        battleStateEnemyLabel.SetActive(false);
    }
    //Funcion que controla si se muestra el display target encima de los enemigos.
    private void ShowHideTargetDisplay(bool showVar)
    {
        GameManager.instance.targetDisplay.SetActive(showVar);
    }

    public void ShowErrorText(string messageToShow)
    {
        errorText.text = messageToShow;
        errorText.gameObject.SetActive(true);
        errorCounter = errorDisplayTime;
    }

    private void ShowHideErrorMessage()
    {
        //Se revisa que el timer del mensaje de error se quite despues de 2f.
        if (errorCounter > 0f)
        {
            errorCounter -= Time.deltaTime;

            errorText.gameObject.transform.position = new Vector3(errorText.gameObject.transform.position.x, (errorText.gameObject.transform.position.y + (50f * Time.deltaTime)), errorText.gameObject.transform.position.z);
            errorText.color = new Color(errorText.color.r, errorText.color.g, errorText.color.b, errorText.color.a - (.6f * Time.deltaTime));

            if (errorCounter <= 0f)
            {
                errorText.gameObject.SetActive(false);
                errorText.gameObject.transform.position = originalPositionErrorText;
                errorText.color = new Color(errorText.color.r, errorText.color.g, errorText.color.b, 1f);
            }
        }
    }
    
    public void ShowFireMenu()
    {
        HideMenus();
        fireMenu.SetActive(true);

        UpdateHitChance();
        SFXManager.instance.UISelect.Play();
    }
    public void HideFireMenu()
    {
        HideMenus();
        ShowInputMenu();

        CameraController.instance.SetMoveTarget(GameManager.instance.activePlayer.transform.position);
        SFXManager.instance.UICancel.Play();
    }

    public void CheckFire()
    {
        GameManager.instance.activePlayer.GetShootTargets();

        if (GameManager.instance.activePlayer.shootTargets.Count > 0) //Si encontre objetivos en rango de disparo.
        {
            ShowFireMenu();
            ShowHideTargetDisplay(true); //Se activa el indicador de Target encima del modelo activo al disparar.
            GameManager.instance.targetDisplay.transform.position = GameManager.instance.activePlayer.shootTargets[GameManager.instance.activePlayer.currentShootTarget].transform.position;
            GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.shootTargets[GameManager.instance.activePlayer.currentShootTarget].transform); //Mandamos rotar al jugador al decidir a cu�l enemigo mirar.

            CameraController.instance.SetFireView(); //Actualizamos la camara de disparo.
        }
        else
        {
            ShowErrorText("No enemies within fire range");
            SFXManager.instance.UICancel.Play();
        }
    }

    public void NextFireTarget()
    {
        GameManager.instance.activePlayer.currentShootTarget++;
        if (GameManager.instance.activePlayer.currentShootTarget >= GameManager.instance.activePlayer.shootTargets.Count)
        {
            GameManager.instance.activePlayer.currentShootTarget = 0;
        }
        GameManager.instance.targetDisplay.transform.position = GameManager.instance.activePlayer.shootTargets[GameManager.instance.activePlayer.currentShootTarget].transform.position;

        UpdateHitChance();

        GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.shootTargets[GameManager.instance.activePlayer.currentShootTarget].transform); //Mandamos rotar al jugador al decidir a cu�l enemigo mirar.

        CameraController.instance.SetFireView(); //Actualizamos la camara de disparo.
        SFXManager.instance.UISelect.Play();
    }

    //Se manda llamar el isparo en el men� de Shoot.
    public void FireShot()
    {
        GameManager.instance.activePlayer.isShooting = true;
        //Se manda llamar la acci�n el jugador activo, se resuelve el costo y comienza una corutina de esperar a la animaci�n.
        GameManager.instance.activePlayer.FireShot();
        GameManager.instance.currentActionCost = 1;
        HideMenus();

        StartCoroutine(WaitToEndActionCo(1f));
        SFXManager.instance.UISelect.Play();
    }

    //Funci�n que controla el texto de las probabilidad de accuracy.
    public void UpdateHitChance()
    {
        hitChanceText.text = "Chance to Hit: " + GameManager.instance.activePlayer.CheckShotChance().ToString("F1") + "%";
    }

    public void Defend()
    {
        GameManager.instance.activePlayer.SetDefending(true);
        GameManager.instance.EndTurn();
        SFXManager.instance.UISelect.Play();
    }

    //Va controlar el inicio de la pelea, cuando todas las unidades hallan sido spawneadas.
    public void StartBattle()
    {
        List<CharacterController> tempList = new List<CharacterController>();
        tempList.AddRange(FindObjectsOfType<CharacterController>()); //Se guarda cada personaje con script de CharacterController(los que se mueven en el plano).

        bool characterFound = false;
        foreach (CharacterController cc in tempList)
        {
            if (cc.isEnemy == false && characterFound == false)
            {
                characterFound = true;
            }
        }
        tempList.Clear();

        if (characterFound == true)
        {
            GameManager.instance.SetUpCharactersInPlay();
            HideMenus();
            startBattleButton.SetActive(false);
            BattleManager.instance.UpdateBattleState(); // Segundo update a battle PLAYERTURN o ENEMYTURN.

            ShowTurnPhases(true); //Se manda true porque es el primer turno.
        }
        else
        {
            ShowErrorText("Debe haber al menos un personaje jugador spawneado en el mapa");
            Debug.Log("Debe haber al menos un personaje jugador spawneado en el mapa");
            SFXManager.instance.UICancel.Play();
        }
    }

    public void LeaveBattle()
    {
        GameManager.instance.LeaveBattle();
    }

    //Se revisa la pausa si esta activo el gameObject de pausa.
    public void PauseUnpause()
    {
        if(pauseScreen.activeSelf == false)
        {
            pauseScreen.SetActive(true);
            GameManager.instance.isPaused = true;
            GameManager.instance.TimeScalingController(0f);
            music.volume /= 3;
            music.pitch = 0.99f;

        }
        else
        {
            pauseScreen.SetActive(false);
            GameManager.instance.isPaused = false;
            GameManager.instance.TimeScalingController(1f);
            music.volume *= 3;
            music.pitch = 1f;
        }
    }

    public void ShowTurnPhases(bool isFirstTurn)
    {
        if (BattleManager.instance.currentBattleState == BattleManager.BattleState.PLAYERTURN)
        {
            battleStateLabel.text = "Hero Phase";
            battleStatePlayerLabel.SetActive(true);
        }
        else if (BattleManager.instance.currentBattleState == BattleManager.BattleState.ENEMYTURN)
        {
            battleStateLabel.text = "Enemy Phase";
            battleStateEnemyLabel.SetActive(true);
        }
        else
        {
            battleStateLabel.gameObject.SetActive(false);
            battleStatePlayerLabel.SetActive(false);
            battleStateEnemyLabel.SetActive(false);
            battleStateLabel.text = "ACASO ENTRE POR AQUI?";
        }
        //battleStateLabel.gameObject.SetActive(true);
        if(isFirstTurn == true)
        {
            StartCoroutine(WaitToStartPhase(1f));
        }
        else
        {
            StartCoroutine(WaitToEndBattlePhases(1f));
        }
    }

    public void UnitsMenu()
    {
        spawnerBG.SetActive(true);
        startBattleButton.SetActive(true);
        moveMapInfoLabel.gameObject.SetActive(false);
        /*if (GameManager.instance.activePlayer.isShooting == false) //Solamente no regreses la camara al personaje activo cuando no estoy disparando.
            CameraController.instance.SetMoveTarget(GameManager.instance.activePlayer.transform.position);
        */
        SFXManager.instance.UISelect.Play();
    }

    public void ShowMapMenu()
    {
        startBattleButton.SetActive(false);
        spawnerBG.SetActive(false);
        startBattleButton.SetActive(false);
        moveMapInfoLabel.gameObject.SetActive(true);
        SFXManager.instance.UISelect.Play();
    }
    public void RestarLevel()
    {
        instance = null;
        Destroy(gameObject);
        GameManager.instance.TimeScalingController(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void GoToMainMenu()
    {
        instance = null;
        Destroy(gameObject);
        GameManager.instance.TimeScalingController(1f);
        SceneManager.LoadScene("Main Menu");
    }

    //Se llamar� cuando el PROLOGO (la cutscene) haya terminado. Se activa START y el menu de units.
    public void CutsceneController()
    {
        isCutsceneOver.SetActive(false);
        UIBattlePhasesActivator();
        BattleManager.instance.UpdateBattleState(); // Primer update a battle START.
    }

    private void UIBattlePhasesActivator()
    {
        switch (BattleManager.instance.currentBattleState)
        {
            case BattleManager.BattleState.PROLOGUE:
                spawnerBG.SetActive(true);
                unitsMenuButton.SetActive(true);
                showMapButton.SetActive(true);
                startBattleButton.SetActive(true);
                break;
        }
    }

}
