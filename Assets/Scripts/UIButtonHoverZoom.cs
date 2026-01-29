using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.06f;   // zoom pequeño
    [SerializeField] private float speed = 16f;          // rapidez del zoom

    private Vector3 baseScale;
    private Vector3 targetScale;

    private void Awake()
    {
        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    private void OnEnable()
    {
        // por si se activa/desactiva el objeto, garantizamos estado limpio
        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = baseScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = baseScale;
    }
}
