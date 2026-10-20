using UnityEngine;
using UnityEngine.InputSystem;

// Jugador 2 - AFD de p2.jff
// Q = {0 idle, 1 adelante, 2 atras, 3 izquierda, 4 derecha, 5 jab, 6 guardia,
//      7 esquiva_izq, 8 esquiva_der, 9 recuperando, 10 golpeado, 11 KO}
// Σ = {Up, Down, Left, Right, K, L, !Up, !Down, !Left, !Right, !K, !L, h, e, t, k}
// q0 = 0   F = {11}
public class P2Controller : MonoBehaviour
{
    Animator animator;

    public GameObject rival;            // objeto del Jugador 1
    public Animator gameAnimator;       // Animator del GameController (opcional)

    public float speed = 2f;
    public float ringLimit = 3f;
    public int hpMax = 100;
    public int damage = 10;
    public float hitRange = 1.5f;
    public float hitTime = 0.5f;
    public float recoverTime = 1f;

    public int hp;
    bool hit;           // símbolo h
    bool dodged;        // símbolo e
    float timer;        // símbolo t
    int lastState;

    Vector3 startPos;
    Quaternion startRot;

    void Start()
    {
        animator = GetComponent<Animator>();
        hp = hpMax;
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void Update()
    {
        // Solo se juega mientras el réferi está en "pelea" (estado 2)
        if (gameAnimator != null && gameAnimator.GetInteger("State") != 2)
        {
            hit = false;
            dodged = false;
            return;
        }

        int state = animator.GetInteger("State");

        bool upPressed = Keyboard.current.upArrowKey.isPressed;
        bool downPressed = Keyboard.current.downArrowKey.isPressed;
        bool leftPressed = Keyboard.current.leftArrowKey.isPressed;
        bool rightPressed = Keyboard.current.rightArrowKey.isPressed;
        bool kPressed = Keyboard.current.kKey.isPressed;
        bool lPressed = Keyboard.current.lKey.isPressed;

        bool h = hit;
        bool e = dodged;
        bool k = hp <= 0;
        hit = false;
        dodged = false;

        // El tiempo t se reinicia cada vez que cambia el estado
        if (state != lastState)
        {
            timer = 0f;
            lastState = state;
        }
        timer += Time.deltaTime;

        switch (state)
        {
            case 0: // idle
                if (h)
                {
                    hp -= damage;
                    animator.SetInteger("State", 10);
                }
                else if (kPressed)
                {
                    animator.SetInteger("State", 5);
                    LanzarJab();
                }
                else if (lPressed)
                {
                    animator.SetInteger("State", 6);
                }
                else if (upPressed)
                {
                    animator.SetInteger("State", 1);
                }
                else if (downPressed)
                {
                    animator.SetInteger("State", 2);
                }
                else if (leftPressed)
                {
                    animator.SetInteger("State", 3);
                }
                else if (rightPressed)
                {
                    animator.SetInteger("State", 4);
                }
                break;

            case 1: // adelante
                if (h)
                {
                    hp -= damage;
                    animator.SetInteger("State", 10);
                }
                else if (!upPressed)
                {
                    animator.SetInteger("State", 0);
                }
                else
                {
                    Move(Vector3.forward);
                }
                break;

            case 2: // atras
                if (h)
                {
                    hp -= damage;
                    animator.SetInteger("State", 10);
                }
                else if (!downPressed)
                {
                    animator.SetInteger("State", 0);
                }
                else
                {
                    Move(Vector3.back);
                }
                break;

            case 3: // izquierda
                if (h)
                {
                    hp -= damage;
                    animator.SetInteger("State", 10);
                }
                else if (leftPressed && lPressed)      // combo Left -> L
                {
                    animator.SetInteger("State", 7);
                }
                else if (!leftPressed)
                {
                    animator.SetInteger("State", 0);
                }
                else
                {
                    Move(Vector3.left);
                }
                break;

            case 4: // derecha
                if (h)
                {
                    hp -= damage;
                    animator.SetInteger("State", 10);
                }
                else if (rightPressed && lPressed)      // combo Right -> L
                {
                    animator.SetInteger("State", 8);
                }
                else if (!rightPressed)
                {
                    animator.SetInteger("State", 0);
                }
                else
                {
                    Move(Vector3.right);
                }
                break;

            case 5: // jab
                if (h)
                {
                    hp -= damage;
                    animator.SetInteger("State", 10);
                }
                else if (e)
                {
                    animator.SetInteger("State", 9);
                }
                else if (!kPressed)
                {
                    animator.SetInteger("State", 0);
                }
                break;

            case 6: // guardia
                if (!lPressed)
                {
                    animator.SetInteger("State", 0);
                }
                break;

            case 7: // esquiva_izq
                if (!lPressed)
                {
                    animator.SetInteger("State", 3);
                }
                break;

            case 8: // esquiva_der
                if (!lPressed)
                {
                    animator.SetInteger("State", 4);
                }
                break;

            case 9: // recuperando
                if (h)
                {
                    hp -= damage;
                    animator.SetInteger("State", 10);
                }
                else if (timer >= recoverTime)     // t
                {
                    animator.SetInteger("State", 0);
                }
                break;

            case 10: // golpeado
                if (k)
                {
                    animator.SetInteger("State", 11);
                }
                else if (timer >= hitTime)         // t
                {
                    animator.SetInteger("State", 0);
                }
                break;

            case 11: // KO
                break;
        }
    }

    // Al entrar a jab se revisa si el golpe conecta
    void LanzarJab()
    {
        if (rival == null) return;
        if (Vector3.Distance(transform.position, rival.transform.position) > hitRange) return;

        int rivalState = rival.GetComponent<Animator>().GetInteger("State");

        if (rivalState == 6 || rivalState == 11)        // guardia o KO
        {
            return;
        }
        else if (rivalState == 7 || rivalState == 8)   // esquiva -> yo recibo e
        {
            dodged = true;
        }
        else                                            // el rival recibe h
        {
            rival.SendMessage("RecibirGolpe");
        }
    }

    public void RecibirGolpe()
    {
        hit = true;
    }

    void Move(Vector3 dir)
    {
        transform.Translate(dir * speed * Time.deltaTime);
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, -ringLimit, ringLimit);
        p.z = Mathf.Clamp(p.z, -ringLimit, ringLimit);
        transform.position = p;
    }

    // Reinicio del autómata a q0 (lo llama el GameController con R)
    public void Reiniciar()
    {
        hp = hpMax;
        hit = false;
        dodged = false;
        timer = 0f;
        lastState = 0;
        transform.SetPositionAndRotation(startPos, startRot);
        animator.Rebind();      // regresa al estado default (idle) con State = 0
    }
}