using UnityEngine;

public class GuideLine : MonoBehaviour
{
    [SerializeField] ParticleSystem deathParticles;

    private void Start()
    {
        LineRenderer line = GetComponent<LineRenderer>();
        //get the angle the guideline makes
        float angle = Mathf.Atan2(line.GetPosition(1).y - line.GetPosition(0).y, line.GetPosition(1).x - line.GetPosition(0).x);
        //rotate the particle system to allign with the line
        deathParticles.gameObject.transform.eulerAngles = new Vector3(Mathf.Rad2Deg * angle, -90, 90);
    }

    /// <summary>
    /// run any visual effects we want the guide line to do, then destroy it
    /// </summary>
    public void DestroySequence()
    {
        //play the particle effect
        deathParticles.Play();
        //separate particles from guideline
        deathParticles.gameObject.transform.parent = null;
        //destory guideline
        Destroy(gameObject);
    }
}
