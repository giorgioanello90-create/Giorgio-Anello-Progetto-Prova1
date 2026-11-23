using UnityEngine;

public class ZonaTrigger : MonoBehaviour
{
    // Proprietà pubblica che indica se il giocatore si trova attualmente nella zona
    public bool GiocatorePresente { get; private set; }

    [Header("Visualizzazione Gizmos")]
    [SerializeField] private Color coloreGizmo = Color.yellow;

    // Quando il Giocatore entra nel CircleCollider2D (Is Trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Giocatore"))
        {
            GiocatorePresente = true;
        }
    }

    // Quando il Giocatore esce dal CircleCollider2D (Is Trigger)
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Giocatore"))
        {
            GiocatorePresente = false;
        }
    }

    // Disegna il cerchio visivo nell'Editor di Unity
    private void OnDrawGizmos()
    {
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            Gizmos.color = coloreGizmo;
            Gizmos.DrawWireSphere(transform.position, circle.radius * transform.localScale.x);
        }
    }
}