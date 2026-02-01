using UnityEngine;

public class BloodGusher : MonoBehaviour
{
    private ParticleSystem ps_;
    
    public float particleSize_;
    public float particleRate_;
    public int id_;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ps_ = gameObject.GetComponentInChildren<ParticleSystem>();
        ps_.GetComponent<ParticleSystemRenderer>().maxParticleSize = particleSize_;
        var emission = ps_.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(0f, particleRate_);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
