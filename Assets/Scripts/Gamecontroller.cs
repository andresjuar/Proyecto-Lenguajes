using UnityEngine;
using UnityEngine.InputSystem;

// Réferi - AFD de game.jff
// Q = {0 esperando, 5 tutorial, 1 cuenta_regresiva, 2 pelea, 3 gana_P2, 4 gana_P1}
// Σ = {Enter, t, k1, k2, R}
// q0 = 0   F = {3, 4}
public class GameController : MonoBehaviour
{
    Animator animator;

    public P1Controller p1;
    public P2Controller p2;

    public float countdownTime = 3f;

    float timer;        // símbolo t
    int lastState;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        int state = animator.GetInteger("State");

        bool enterPressed = Keyboard.current.enterKey.wasPressedThisFrame;
        bool rPressed = Keyboard.current.rKey.wasPressedThisFrame;
        bool k1 = p1.GetComponent<Animator>().GetInteger("State") == 11;   // P1 en KO
        bool k2 = p2.GetComponent<Animator>().GetInteger("State") == 11;   // P2 en KO

        if (state != lastState)
        {
            timer = 0f;
            lastState = state;
        }
        timer += Time.deltaTime;

        switch (state)
        {
            case 0: // Pantalla de Inicio
                if (enterPressed)
                {
                    animator.SetInteger("State", 5); // Pasa a tutorial
                }
                break;

            case 5: // Tutorial / Controles
                if (enterPressed)
                {
                    animator.SetInteger("State", 1); // Pasa a cuenta regresiva
                }
                break;

            case 1: // Cuenta regresiva
                if (timer >= countdownTime)         // t
                {
                    animator.SetInteger("State", 2);
                }
                break;

            case 2: // Pelea
                if (k1)
                {
                    animator.SetInteger("State", 3);
                }
                else if (k2)
                {
                    animator.SetInteger("State", 4);
                }
                break;

            case 3: // Gana_P2
                if (rPressed)
                {
                    p1.Reiniciar();
                    p2.Reiniciar();
                    animator.SetInteger("State", 0);
                }
                break;

            case 4: // Gana_P1
                if (rPressed)
                {
                    p1.Reiniciar();
                    p2.Reiniciar();
                    animator.SetInteger("State", 0);
                }
                break;
        }
    }

    // Texto en pantalla
    void OnGUI()
    {
        if (animator == null) return;
        int state = animator.GetInteger("State");

        GUIStyle big = new GUIStyle(GUI.skin.label);
        big.fontSize = 40;
        big.alignment = TextAnchor.MiddleCenter;

        // Aumentamos la altura de la caja contenedora para evitar el recorte del texto
        Rect center = new Rect(0, Screen.height / 2 - 200, Screen.width, 400);

        GUIStyle small = new GUIStyle(GUI.skin.label);
        small.fontSize = 22;

        // Dibujar HP solo durante o después de la pelea
        if (state != 0 && state != 5)
        {
            GUI.Label(new Rect(20, 10, 300, 40), "P1  HP: " + p1.hp, small);
            GUI.Label(new Rect(Screen.width / 2 + 20, 10, 300, 40), "P2  HP: " + p2.hp, small);
        }

        switch (state)
        {
            case 0:
                GUI.Label(center, "Knock-Out!\nPresiona ENTER para empezar", big);
                break;

            case 5:
                // Usamos un tamaño de letra adecuado para textos de varias líneas
                GUIStyle tutorialStyle = new GUIStyle(GUI.skin.label);
                tutorialStyle.fontSize = 28; 
                tutorialStyle.alignment = TextAnchor.MiddleCenter;

                string tutorialText = "=== CONTROLES ===\n\n" +
                                     "P1: Mover (W,A,S,D) | Jab (F) | Guardia (G)\n" +
                                     "P2: Mover (Flechas) | Jab (K) | Guardia (L)\n\n" +
                                     "Presiona ENTER para empezar";
                GUI.Label(center, tutorialText, tutorialStyle);
                break;

            case 1:
                GUI.Label(center, Mathf.Ceil(countdownTime - timer).ToString(), big);
                break;

            case 3:
                GUI.Label(center, "¡GANA JUGADOR 2!\n(R para reiniciar)", big);
                break;

            case 4:
                GUI.Label(center, "¡GANA JUGADOR 1!\n(R para reiniciar)", big);
                break;
        }
    }
}