using UnityEngine; // Libreria principale di Unity
using UnityEngine.InputSystem; // Libreria per gestire il nuovo sistema di input di Unity

// RequireComponent obbliga il GameObject ad avere un componente Rigidbody2D.
// Se non è presente, Unity lo aggiunge automaticamente quando associamo lo script.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof(Animator))]
public class MovimentoGiocatore : MonoBehaviour
{
    // SerializeField permette di modificare una variabile privata dall'Inspector di Unity.
    // La velocità iniziale del personaggio è impostata a 5.
    [SerializeField] private float velocita = 5f;

    // Riferimento al componente Rigidbody2D del personaggio.
    // Ci servirà per gestire il movimento attraverso il sistema fisico di Unity.
    private Rigidbody2D corpo;
    private Animator animatore;

    private SpriteRenderer spriteRenderer;

    // Rappresenta l'azione "Move" configurata nel nuovo Input System.
    // L'azione può essere associata a WASD, frecce direzionali o controller.
    private InputAction azioneMovimento;

    // Vector2 rappresenta un vettore con due componenti: X e Y.
    // X indica il movimento orizzontale, Y quello verticale.
    // (0, 0) = fermo
    // (1, 0) = destra
    // (-1, 0) = sinistra
    // (0, 1) = alto
    // (0, -1) = basso
    private Vector2 direzione;

    // Awake viene eseguito durante l'inizializzazione del componente.
    // Lo utilizziamo per recuperare i riferimenti necessari allo script.
    private void Awake()
    {
        // GetComponent<T>() cerca un componente del tipo specificato
        // all'interno dello stesso GameObject su cui è presente lo script.
        // In questo caso recuperiamo il Rigidbody2D.
        corpo = GetComponent<Rigidbody2D>();

        animatore = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        // InputSystem.actions permette di accedere alle azioni globali
        // configurate nel progetto attraverso il nuovo Input System.

        // FindAction cerca l'azione "Move" nella Action Map "Player".
        // Il parametro true indica che Unity deve generare un errore
        // se l'azione richiesta non viene trovata.
        azioneMovimento = InputSystem.actions.FindAction("Player/Move", true);
    }

    // Update viene eseguito una volta per ogni frame.
    // È adatto alla lettura degli input del giocatore.
    private void Update()
    {
        // ReadValue<Vector2>() legge il valore corrente dell'azione Move.
        // Il risultato viene salvato nella variabile direzione.
        //
        // Esempio:
        // Premiamo D -> direzione = (1, 0)
        // Premiamo A -> direzione = (-1, 0)
        // Non premiamo nulla -> direzione = (0, 0)
        //
        // Non stiamo ancora muovendo il personaggio:
        // stiamo soltanto leggendo la direzione desiderata.
        direzione = azioneMovimento.ReadValue<Vector2>();

        animatore.SetFloat("MoveX", direzione.x);
        animatore.SetFloat("MoveY", direzione.y);

        bool inMovimento = direzione.sqrMagnitude > 0.001f;

        animatore.SetBool("IsMoving", inMovimento);

        if(direzione.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if(direzione.x > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    // FixedUpdate viene eseguito a intervalli di tempo fissi.
    // Per impostazione predefinita, generalmente ogni 0.02 secondi.
    // Viene utilizzato per aggiornare il movimento fisico.
    private void FixedUpdate()
    {
        // linearVelocity rappresenta la velocità lineare del Rigidbody2D.
        // È una proprietà espressa in unità di Unity al secondo.
        //
        // Moltiplichiamo la direzione per la velocità impostata.
        //
        // Esempio:
        // direzione = (1, 0)
        // velocita = 5
        // linearVelocity = (5, 0)
        //
        // Il personaggio si muoverà verso destra a 5 unità al secondo.
        //
        // Se direzione = (0, 0), la velocità diventa (0, 0)
        // e il personaggio si ferma.
        //
        // Non utilizziamo Time.deltaTime perché stiamo assegnando
        // direttamente una velocità, non uno spostamento.
        corpo.linearVelocity = direzione * velocita;
    }
}