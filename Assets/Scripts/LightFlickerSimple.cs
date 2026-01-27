using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlickerSimple : MonoBehaviour
{
    [SerializeField] private float variation = 10f;      // cuánto sube/baja la intensidad
    [SerializeField] private float minInterval = 0.05f;  // tiempo mínimo entre cambios
    [SerializeField] private float maxInterval = 0.20f;  // tiempo máximo entre cambios
    [SerializeField] private float smoothSpeed = 12f;    // suavidad (más = cambios más rápidos)

    private Light _light;
    private float _baseIntensity;
    private float _targetIntensity;
    private float _timer;

    private void Awake()
    {
        _light = GetComponent<Light>();
        _baseIntensity = _light.intensity;
        PickNewTarget();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
            PickNewTarget();

        _light.intensity = Mathf.Lerp(_light.intensity, _targetIntensity, Time.deltaTime * smoothSpeed);
    }

    private void PickNewTarget()
    {
        _timer = Random.Range(minInterval, maxInterval);
        _targetIntensity = _baseIntensity + Random.Range(-variation, variation);
        if (_targetIntensity < 0f) _targetIntensity = 0f;
    }
}
