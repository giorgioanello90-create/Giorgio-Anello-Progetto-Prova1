
using UnityEngine; // Libreria principale di Unity.
using UnityEngine.InputSystem; // Libreria del nuovo Input System.

// RequireComponent obbliga il GameObject ad avere
// un Rigidbody2D e un Animator.
// Se mancano, Unity li aggiunge automaticamente.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class MovimentoGiocatore2 : MonoBehaviour
{
    // Velocità di movimento del personaggio.
    // SerializeField permette di modificarla dall'Inspector.
    [SerializeField] private float velocita = 5f;

    // Riferimento al Rigidbody2D del personaggio.
    // Viene utilizzato per applicare il movimento fisico.
    private Rigidbody2D corpo;

    // Riferimento all'Animator.
    // Gestisce le animazioni Idle e Walk attraverso i Blend Tree.
    private Animator animatore;

    // Azione Move configurata nel nuovo Input System.
    // Permette di leggere WASD, frecce direzionali o controller.
    private InputAction azioneMovimento;

    // Vector2 contiene due coordinate: X e Y.
    //
    // (0, 0)   = fermo
    // (0, 1)   = alto
    // (0, -1)  = basso
    // (-1, 0)  = sinistra
    // (1, 0)   = destra
    //
    // Combinando X e Y possiamo muoverci anche in diagonale.
    private Vector2 direzione;


    // -------------------------------------------------------
    // AWAKE - INIZIALIZZAZIONE
    // -------------------------------------------------------

    // Awake viene eseguito durante l'inizializzazione
    // del componente, prima del suo primo Update.
    private void Awake()
    {
        // Recuperiamo il Rigidbody2D del GameObject.
        corpo = GetComponent<Rigidbody2D>();

        // Recuperiamo l'Animator del GameObject.
        animatore = GetComponent<Animator>();

        // Cerchiamo l'azione Move nella Action Map Player.
        // true genera un errore se l'azione non esiste.
        azioneMovimento = InputSystem.actions.FindAction("Player/Move", true);
    }


    // -------------------------------------------------------
    // UPDATE - INPUT E ANIMAZIONI
    // -------------------------------------------------------

    // Update viene eseguito una volta per ogni frame.
    // Lo utilizziamo per leggere gli input
    // e aggiornare i parametri dell'Animator.
    private void Update()
    {
        // Leggiamo la direzione dal nuovo Input System.
        // Il risultato è un Vector2 con coordinate X e Y.
        direzione = azioneMovimento.ReadValue<Vector2>();

        // Limitiamo la lunghezza massima del vettore a 1.
        //
        // Senza questa operazione, premendo due tasti
        // contemporaneamente (es. W + D), il personaggio
        // potrebbe muoversi più velocemente in diagonale.
        //
        // ClampMagnitude mantiene la direzione originale,
        // ma impedisce che la lunghezza superi 1.
        direzione = Vector2.ClampMagnitude(direzione, 1f);


        // ---------------------------------------------------
        // CONTROLLO DEL MOVIMENTO
        // ---------------------------------------------------

        // sqrMagnitude restituisce la lunghezza
        // del vettore elevata al quadrato.
        //
        // Formula: X * X + Y * Y
        //
        // Se il valore è praticamente zero,
        // significa che il personaggio è fermo.
        //
        // Utilizziamo una piccola soglia per ignorare
        // eventuali movimenti minimi di un controller.
        bool inMovimento = direzione.sqrMagnitude > 0.001f;


        // ---------------------------------------------------
        // AGGIORNAMENTO DEL BLEND TREE
        // ---------------------------------------------------

        // Aggiorniamo i parametri direzionali
        // soltanto quando il personaggio si muove.
        if (inMovimento)
        {
            // normalized restituisce un vettore
            // con lunghezza uguale a 1.
            //
            // Esempi:
            //
            // Destra:       (1, 0)
            // Sinistra:     (-1, 0)
            // Alto:         (0, 1)
            // Basso:        (0, -1)
            // Alto-destra:  (0.707, 0.707)
            //
            // Questo permette di mantenere una direzione
            // coerente per il Blend Tree.
            Vector2 orientamento = direzione.normalized;

            // MoveX controlla l'orientamento orizzontale.
            //
            // -1 = sinistra
            //  0 = nessuna componente orizzontale
            //  1 = destra
            animatore.SetFloat("MoveX", orientamento.x);

            // MoveY controlla l'orientamento verticale.
            //
            // -1 = basso
            //  0 = nessuna componente verticale
            //  1 = alto
            animatore.SetFloat("MoveY", orientamento.y);
        }

        // Quando il personaggio si ferma,
        // MoveX e MoveY non vengono aggiornati.
        //
        // In questo modo il personaggio mantiene
        // l'ultima direzione verso cui stava guardando.


        // ---------------------------------------------------
        // PASSAGGIO TRA IDLE E WALK
        // ---------------------------------------------------

        // isMoving è un parametro Bool dell'Animator.
        //
        // true  = il personaggio si muove -> Walk
        // false = il personaggio è fermo  -> Idle
        //
        // Le transizioni tra gli stati vengono gestite
        // automaticamente dall'Animator Controller.
        animatore.SetBool("isMoving", inMovimento);
    }


    // -------------------------------------------------------
    // FIXEDUPDATE - MOVIMENTO FISICO
    // -------------------------------------------------------

    // FixedUpdate viene eseguito a intervalli di tempo fissi.
    // È utilizzato per aggiornare il sistema fisico di Unity.
    private void FixedUpdate()
    {
        // linearVelocity rappresenta la velocità
        // del Rigidbody2D sugli assi X e Y.
        //
        // Moltiplichiamo la direzione per la velocità.
        //
        // Esempio:
        //
        // direzione = (1, 0)
        // velocita = 5
        //
        // Risultato = (5, 0)
        // Il personaggio si muove verso destra.
        //
        // Non utilizziamo Time.deltaTime perché
        // linearVelocity è già espressa in unità al secondo.
        corpo.linearVelocity = direzione * velocita;
    }
}